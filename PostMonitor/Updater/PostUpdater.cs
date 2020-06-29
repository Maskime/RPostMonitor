using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using AutoMapper;

using Common.Model.Document;
using Common.Model.Repositories;
using Common.Reddit;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PostMonitor.Config;

using Timer = System.Timers.Timer;

namespace PostMonitor.Updater
{
    public class PostUpdater:IHostedService, IDisposable
    {
        private readonly UpdaterConfiguration _config;
        private readonly IMonitoredPostRepository _repo;
        private readonly ILogger<PostUpdater> _logger;
        private readonly IRedditWrapper _wrapper;
        private IMapper _mapper;
        private int _updateInstance = 0;
        private Timer _timer;

        public PostUpdater(ILogger<PostUpdater> logger
            ,IOptions<UpdaterConfiguration> configOption
            , IMonitoredPostRepository repo
            , IRedditWrapper wrapper
            , IMapper mapper
        )
        {
            _logger = logger;
            _config = configOption.Value;
            _repo = repo;
            _wrapper = wrapper;
            _mapper = mapper;
        }

        private void UpdatePosts(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _timer.Enabled = false;
            _updateInstance++;
            _logger.LogDebug($"Starting instance [{_updateInstance}]");
            
            List<IMonitoredPost> postToUpdate = _repo.FindPostWithLastFetchedOlderThan(60);
            if (postToUpdate == null)
            {
                _timer.Enabled = true;
                return;
            }

            List<List<IMonitoredPost>> paginatedResults = PaginatePostToUpdateList(postToUpdate);
            foreach (List<IMonitoredPost> resultsPage in paginatedResults)
            {
                var tasks = new List<Task<IRedditPost>>(resultsPage.Count);
                foreach (IMonitoredPost monitoredPost in resultsPage)
                {
                    _logger.LogDebug($"Planning to fetch [{monitoredPost.Title}]Iteration Number [{monitoredPost.IterationsNumber}] Last fetch [{monitoredPost.FetchedAt}]");
                    tasks.Add(_wrapper.FetchAsync(monitoredPost.FullName));
                }

                try
                {
                    _logger.LogDebug($"Waiting for the [{tasks.Count}] to finish");
                    Task.WaitAll(tasks: tasks.ToArray());
                    foreach (Task<IRedditPost> task in tasks)
                    {
                        IRedditPost postUpdate = task.Result;
                        _repo.AddVersion(_mapper.Map<IMonitoredPost>(postUpdate));
                        _logger.LogDebug($"Fetched [{postUpdate.Title}] [{postUpdate.Id}]");
                    }
                }
                catch (AggregateException aggregateException)
                {
                    _logger.LogError(@"Errors when fetching several posts at  the same time.");
                    foreach (Exception exception in aggregateException.InnerExceptions)
                    {
                        _logger.LogError(exception, @"When fetching post details");
                    }
                }
            }

            _timer.Enabled = true;
        }

        private List<List<IMonitoredPost>> PaginatePostToUpdateList(List<IMonitoredPost> postToUpdate)
        {
            var output = new List<List<IMonitoredPost>>();
            int index = 0;
            foreach (IMonitoredPost monitoredPost in postToUpdate)
            {
                if (index == output.Count || index == output.Count + 1)
                {
                    output.Insert(index, new List<IMonitoredPost>());
                }
                
                output[index].Add(monitoredPost);
                if (output[index].Count > 0 && output[index].Count % _config.SimultaneousFetchRequest == 0)
                {
                    index++;
                }
            }

            return output;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");
            _timer = new Timer { 
                Interval = _config.PeriodicityInSeconds * 1000, 
                AutoReset = true, 
                Enabled = true
            };
            _timer.Elapsed += UpdatePosts;
            // _timer = new Timer(UpdatePosts, null, TimeSpan.Zero, TimeSpan.FromSeconds(_config.PeriodicityInSeconds));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer.Enabled = false;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}