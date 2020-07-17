using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using AutoMapper;

using Common.Errors;
using Common.Model.Document;
using Common.Model.Repositories;
using Common.Reddit;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PostMonitor.Config;

using Timer = System.Timers.Timer;

namespace PostMonitor.HostedServices
{
    public class PostUpdater : IHostedService, IDisposable
    {
        private readonly UpdaterConfiguration _config;
        private readonly IMonitoredPostRepository _repo;
        private readonly ILogger<PostUpdater> _logger;
        private readonly IRedditClientWrapper _clientWrapper;
        private IMapper _mapper;
        private Timer _timer;
        private CancellationToken _cancellationToken;

        public PostUpdater(ILogger<PostUpdater> logger
            , IOptions<UpdaterConfiguration> configOption
            , IMonitoredPostRepository repo
            , IRedditClientWrapper clientWrapper
            , IMapper mapper
        )
        {
            _logger = logger;
            _config = configOption.Value;
            _repo = repo;
            _clientWrapper = clientWrapper;
            _mapper = mapper;
        }
        
        public Task StartAsync(CancellationToken stoppingToken)
        {
            _cancellationToken = stoppingToken;
            _logger.LogInformation("Resetting IsFetching flag to false.");
            _repo.SetFetchingAll(false);
            _timer = new Timer
            {
                Interval = _config.PeriodicityInSeconds * 1000,
                AutoReset = true,
                Enabled = true
            };
            _timer.Elapsed += UpdatePosts;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer.Enabled = false;

            return Task.CompletedTask;
        }

        private void UpdatePosts(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            List<IRedditMonitoredPost> postToUpdate = _repo
                .FindPostToUpdate(
                    _config.TimeBetweenFetchInSeconds,
                    _config.NbIterationOnPost,
                    _config.InactivityTimeoutInHours,
                    _config.SimultaneousFetchRequest);
            _logger.LogInformation(@"Updating [{Count}] posts", postToUpdate.Count);

            var tasks = new List<Task<IRedditPost>>(_config.SimultaneousFetchRequest);
            foreach (IRedditMonitoredPost monitoredPost in postToUpdate)
            {
                _logger.LogDebug(@"Fetching [{FullName}] [{@MonitoredPost}]",
                    monitoredPost.FullName, monitoredPost);
                _repo.SetFetching(monitoredPost.FullName, true);
                tasks.Add(_clientWrapper.FetchAsync(monitoredPost.FullName));
            }

            try
            {
                _logger.LogDebug(@"Started [{Count}] fetching tasks", tasks.Count);
                Task.WaitAll(tasks.ToArray());
                HandleFetchedPosts(tasks);
            }
            catch (AggregateException aggregateException)
            {
                _logger.LogError(@"Errors when fetching several posts at  the same time.");
                foreach (Exception exception in aggregateException.InnerExceptions)
                {
                    _logger.LogError(exception, @"When fetching post details");
                }
            }
            finally
            {
                foreach (IRedditMonitoredPost redditMonitoredPost in postToUpdate)
                {
                    _repo.SetFetching(redditMonitoredPost.FullName, false);
                }
            }
        }

        private void HandleFetchedPosts(List<Task<IRedditPost>> tasks)
        {
            foreach (IRedditPost fetchedPost in tasks.Select(task => task.Result))
            {
                if (fetchedPost == null)
                {
                    throw new PostMonitorException(@"Fetched post is null");
                }
                IRedditMonitoredPost lastVersion = _repo.Get(fetchedPost.FullName);
                if (AreEqual(fetchedPost, lastVersion))
                {
                    _repo.UpdatePostInactivity(lastVersion);
                    _logger.LogDebug(@"No modification for post [{FullName}], Inactivity updated", lastVersion.FullName);
                }
                else
                {
                    _repo.AddVersion(fetchedPost);
                    _logger.LogDebug(@"New version added for post [{FullName}]", lastVersion.FullName);
                }

                _logger.LogDebug(@"Resetting IsFetching flag for [{FullName}]", lastVersion.FullName);
                _repo.SetFetching(fetchedPost.FullName, false);
            }
        }

        private bool AreEqual(IRedditPost fetchedPost, IRedditMonitoredPost lastVersion)
        {
            if (lastVersion == null)
            {
                return false;
            }
            return    fetchedPost.NumReports == lastVersion.NumReports 
                   && fetchedPost.ReportCount == lastVersion.ReportCount 
                   && fetchedPost.ApprovedBy == lastVersion.ApprovedBy 
                   && fetchedPost.AuthorFlairCssClass == lastVersion.AuthorFlairCssClass 
                   && fetchedPost.AuthorFlairText == lastVersion.AuthorFlairText 
                   && fetchedPost.AuthorName == lastVersion.AuthorName 
                   && fetchedPost.BannedBy == lastVersion.BannedBy 
                   && fetchedPost.CommentCount == lastVersion.CommentCount 
                   && fetchedPost.Domain == lastVersion.Domain 
                   && fetchedPost.Downvotes == lastVersion.Downvotes 
                   && fetchedPost.Edited == lastVersion.Edited 
                   && fetchedPost.FullName == lastVersion.FullName 
                   && fetchedPost.Gilded == lastVersion.Gilded 
                   && fetchedPost.IsArchived == lastVersion.IsArchived 
                   && fetchedPost.IsSelfPost == lastVersion.IsSelfPost 
                   && fetchedPost.IsSpoiler == lastVersion.IsSpoiler 
                   && fetchedPost.IsStickied == lastVersion.IsStickied 
                   && fetchedPost.Kind == lastVersion.Kind 
                   && fetchedPost.LinkFlairCssClass == lastVersion.LinkFlairCssClass 
                   && fetchedPost.LinkFlairText == lastVersion.LinkFlairText 
                   && fetchedPost.NSFW == lastVersion.NSFW 
                   && fetchedPost.Saved == lastVersion.Saved 
                   && fetchedPost.Score == lastVersion.Score 
                   && fetchedPost.SelfText == lastVersion.SelfText 
                   && fetchedPost.SelfTextHtml == lastVersion.SelfTextHtml 
                   && fetchedPost.Shortlink == lastVersion.Shortlink 
                   && fetchedPost.SubredditName == lastVersion.SubredditName 
                   && fetchedPost.Title == lastVersion.Title 
                   && fetchedPost.Upvotes == lastVersion.Upvotes 
                   && Equals(fetchedPost.Permalink, lastVersion.Permalink) 
                   && Equals(fetchedPost.Thumbnail, lastVersion.Thumbnail) 
                   && Equals(fetchedPost.Url, lastVersion.Url);
        }

        private List<List<IRedditMonitoredPost>> PaginatePostToUpdateList(List<IRedditMonitoredPost> postToUpdate)
        {
            var output = new List<List<IRedditMonitoredPost>>();
            int index = 0;
            foreach (IRedditMonitoredPost monitoredPost in postToUpdate)
            {
                if (index == output.Count || index == output.Count + 1)
                {
                    output.Insert(index, new List<IRedditMonitoredPost>());
                }

                output[index].Add(monitoredPost);
                if (output[index].Count > 0 && output[index].Count % _config.SimultaneousFetchRequest == 0)
                {
                    index++;
                }
            }

            return output;
        }

        

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
