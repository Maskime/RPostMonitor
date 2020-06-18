// ************************************************************************************************
// 
//  © 2019       General Electric Company
// 
//  Description  See class summary below.
// 
//  History      See source code control system.
// 
// ************************************************************************************************

using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Config;
using Common.Model.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedditSharp;
using RedditSharp.Things;

namespace PostPoller
{
    public class RGoddessesPoller : IHostedService
    {
        private readonly ILogger<RGoddessesPoller> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IApplicationConfiguration _appConfig;
        private IMonitoredPostRepository _monitoredPostRepository;

        public RGoddessesPoller(
            ILogger<RGoddessesPoller> logger,
            IHostApplicationLifetime applicationLifetime,
            IApplicationConfiguration appConfig
            // , IMonitoredPostRepository monitoredPostRepository
            )
        {
            this._logger = logger;
            this._applicationLifetime = applicationLifetime;
            this._appConfig = appConfig;
            // _monitoredPostRepository = monitoredPostRepository;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug(@"StartAsync called");
            _applicationLifetime.ApplicationStarted.Register(OnStarted);
            _applicationLifetime.ApplicationStopped.Register(OnStopped);
            _applicationLifetime.ApplicationStopping.Register(OnStopping);
            _logger.LogDebug($"Configuration values [{_appConfig.Name}]");
            await StartPolling(cancellationToken);
        }

        private async Task StartPolling(CancellationToken cancellationToken)
        {
            IRedditConfiguration redditConfig = _appConfig.Reddit;
            var webAgent = new BotWebAgent(redditConfig.Username, redditConfig.UserPassword, redditConfig.ClientId,
                redditConfig.ClientSecret, redditConfig.RedirectURI);
            //This will check if the access token is about to expire before each request and automatically request a new one for you
            //"false" means that it will NOT load the logged in user profile so reddit.User will be null
            var reddit = new Reddit(webAgent, true);
            Subreddit subreddit = await reddit.GetSubredditAsync("/r/goddesses");
            ListingStream<Post> postStream = subreddit.GetPosts(Subreddit.Sort.New)
                                                      .Stream();
            postStream.Subscribe(HandlingPost);
            await postStream.Enumerate(cancellationToken);
        }

        private void HandlingPost(Post post)
        {
            // _monitoredPostRepository.Insert(new MonitoredPost
            // {
            //     Author = post.AuthorName,
            //     CreatedAt = DateTime.Now,
            //     FetchedAt = post.FetchedAt,
            //     Permalink = post.Permalink.ToString(),
            //     RedditId = post.Id,
            //     Url = post.Url.ToString()
            // });
            _logger.LogDebug($"Post : [{post.Title} at {post.CreatedUTC}]");
        }

        private void OnStopping()
        {
            _logger.LogInformation($"Stopping Poller");
        }

        private void OnStopped()
        {
            _logger.LogInformation($"Poller Stopped");
        }

        private void OnStarted()
        {
            _logger.LogInformation($"Poller Started");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug(@"StopAsync called");
            return Task.CompletedTask;
        }
    }
}
