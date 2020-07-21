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

using Common.Errors;
using Common.Model.Repositories;
using Common.Reddit;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PostMonitor.Config;

namespace PostMonitor.HostedServices
{
    public class PostPoller : IHostedService
    {
        private readonly ILogger<PostPoller> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly PollerConfiguration _pollerConfig;
        private readonly IMonitoredPostRepository _monitoredPostRepository;
        private readonly IRedditClientWrapper _redditClientWrapper;
        private bool _pollerStarted;
        private TimeSpan _newPostMaxAge;

        public PostPoller(
            ILogger<PostPoller> logger
            ,IHostApplicationLifetime applicationLifetime
            ,IOptions<PollerConfiguration> config
            ,IMonitoredPostRepository monitoredPostRepository
            ,IRedditClientWrapper redditClientWrapper
            )
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _pollerConfig = config.Value;
            _monitoredPostRepository = monitoredPostRepository;
            _redditClientWrapper = redditClientWrapper;
            _redditClientWrapper.ConnectivityUpdated += ConnectivityUpdated;
            _newPostMaxAge = TimeSpan.FromMinutes(_pollerConfig.NewPostMaxAgeInMinutes);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _applicationLifetime.ApplicationStopping.Register(OnStopping);
            return Task.CompletedTask;
        }

        private async Task StartPolling()
        {
            if (!NeedToPoll())
            {
                _logger.LogInformation("[Start]We already reached the number of post that are to be monitored");
                return;
            }

            _pollerStarted = true;
            _logger.LogInformation("Starting polling subreddit [{SubReddit}]", _pollerConfig.SubToWatch);
            await _redditClientWrapper.ListenToNewPosts(_pollerConfig.SubToWatch, OnNext, OnError);
        }

        private void OnError(PostMonitorException obj)
        {
            throw obj;
        }

        private void OnNext(IRedditPost post)
        {
            TimeSpan age = DateTimeOffset.UtcNow - post.CreatedUTC;
            if (age > _newPostMaxAge)
            {
                _logger.LogInformation("Post [{FullName}] is discarded because it's already too old", post.FullName);
                return;
            }
            if (!_monitoredPostRepository.Insert(post))
            {
                _logger.LogWarning("For some reason, could not create the monitored post");
                return;
            }
            _logger.LogInformation(@"Adding post [{FullName}] to watch list [{CurrentCount}/{TargetCount}]", 
                post.FullName, 
                _monitoredPostRepository.CountMonitoredPosts(), 
                _pollerConfig.NbPostToMonitor);
            if (!NeedToPoll())
            {
                _logger.LogInformation("[OnNext]We've reached the number of post that are to be monitored, Stopping poller on this sub");
                _redditClientWrapper.StopListeningToNewPost(_pollerConfig.SubToWatch);
            }
        }

        private bool NeedToPoll()
        {
            return _monitoredPostRepository.CountMonitoredPosts() < _pollerConfig.NbPostToMonitor;
        }

        private void OnStopping()
        {
            StopPolling();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug(@"StopAsync called");
            StopPolling();
            return Task.CompletedTask;
        }
        
        private async void ConnectivityUpdated(bool status)
        {
            if (status && !_pollerStarted)
            {
                await StartPolling();
            }
            else if (!status && _pollerStarted)
            {
                StopPolling();
            }
        }

        private void StopPolling()
        {
            _logger.LogInformation($"Stopping Poller");
            _redditClientWrapper.StopListeningToNewPost(_pollerConfig.SubToWatch);
            _pollerStarted = false;
        }
    }
}
