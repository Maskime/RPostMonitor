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

using Common.Model.Repositories;
using Common.Reddit;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PostMonitor.Config;

namespace PostMonitor.Poller
{
    public class PostPoller : IHostedService
    {
        private readonly ILogger<PostPoller> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly PollerConfiguration _appConfig;
        private readonly IMonitoredPostRepository _monitoredPostRepository;
        private readonly IRedditWrapper _redditWrapper;

        public PostPoller(
            ILogger<PostPoller> logger,
            IHostApplicationLifetime applicationLifetime,
            IOptions<PollerConfiguration> config,
            IMonitoredPostRepository monitoredPostRepository,
            IRedditWrapper redditWrapper
            )
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _appConfig = config.Value;
            _monitoredPostRepository = monitoredPostRepository;
            _redditWrapper = redditWrapper;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug(@"StartAsync called");
            _applicationLifetime.ApplicationStarted.Register(OnStarted);
            _applicationLifetime.ApplicationStopped.Register(OnStopped);
            _applicationLifetime.ApplicationStopping.Register(OnStopping);
            await StartPolling(cancellationToken);
        }

        private async Task StartPolling(CancellationToken cancellationToken)
        {
            await _redditWrapper.ListenToNewPosts("/r/goddesses", HandlingPost);
        }

        private void HandlingPost(IRedditPost post)
        {
            _monitoredPostRepository.Insert(new MonitoredPost
            {
                Author = post.Author,
                CreatedAt = DateTime.Now,
                FetchedAt = post.FetchedAt,
                Permalink = post.Permalink,
                RedditId = post.RedditId,
                Url = post.Url
            });
            _logger.LogDebug($"Post : [{post.Title} at {post.CreatedAt}]");
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
