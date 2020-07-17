// ************************************************************************************************
// 
//  © 2019       General Electric Company
// 
//  Description  See class summary below.
// 
//  History      See source code control system.
// 
// ************************************************************************************************

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
        private readonly PollerConfiguration _pollerConfig;
        private readonly IMonitoredPostRepository _monitoredPostRepository;
        private readonly IRedditClientWrapper _redditClientWrapper;

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
            if (_pollerConfig.NbPostToMonitor - _monitoredPostRepository.CountMonitoredPosts() <= 0)
            {
                _logger.LogInformation("We already reached the number of post that are to be monitored");
                return;
            }

            await _redditClientWrapper.ListenToNewPosts(_pollerConfig.SubToWatch, HandlingPost);
        }

        private void HandlingPost(IRedditPost post)
        {
            if (_monitoredPostRepository.CountMonitoredPosts() + 1 > _pollerConfig.NbPostToMonitor)
            {
                _logger.LogInformation("We already reached the number of post that are to be monitored, Stopping poller on this sub");
                _redditClientWrapper.StopListeningToNewPost(_pollerConfig.SubToWatch);
                return;
            }
            
            if (_monitoredPostRepository.Insert(post))
            {
                _logger.LogInformation(@"Adding post [{FullName}] to watch list [{CurrentCount}/{TargetCount}]", 
                    post.FullName, 
                    _monitoredPostRepository.CountMonitoredPosts(), 
                    _pollerConfig.NbPostToMonitor);
            }
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
