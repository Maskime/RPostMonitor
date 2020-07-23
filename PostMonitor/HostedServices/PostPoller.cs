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
using System.Timers;

using Common.Errors;
using Common.Model.Repositories;
using Common.Reddit;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PostMonitor.Config;

using Timer = System.Timers.Timer;

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
        private Timer _timer;
        private IWatchedSubRedditRepository _watchedSubRedditRepository;
        private TimeSpan _subRedditWatchTime;

        public PostPoller(
            ILogger<PostPoller> logger
            ,IHostApplicationLifetime applicationLifetime
            ,IOptions<PollerConfiguration> config
            ,IMonitoredPostRepository monitoredPostRepository
            ,IRedditClientWrapper redditClientWrapper
            ,IWatchedSubRedditRepository watchedSubRedditRepository
            )
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _pollerConfig = config.Value;
            _monitoredPostRepository = monitoredPostRepository;
            _watchedSubRedditRepository = watchedSubRedditRepository;
            _redditClientWrapper = redditClientWrapper;
            _redditClientWrapper.ConnectivityUpdated += ConnectivityUpdated;
            _newPostMaxAge = TimeSpan.FromMinutes(_pollerConfig.NewPostMaxAgeInMinutes);
            _subRedditWatchTime = TimeSpan.FromHours(_pollerConfig.SubRedditWatchTimeInHours);
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
            _timer = new Timer()
            {
                Enabled = true,
                AutoReset = true,
                Interval = TimeSpan.FromMinutes(1).TotalMilliseconds
            };
            var progress = new Progress<Exception>();
            _timer.Elapsed += (sender, args) => { UpdateWatchedTime(progress, _pollerConfig.SubToWatch);};
            progress.ProgressChanged += (sender, exception) => throw exception;
            _watchedSubRedditRepository.UpdatedPollerStartedAt(_pollerConfig.SubToWatch);
            await _redditClientWrapper.ListenToNewPosts(_pollerConfig.SubToWatch, OnNext, OnError);
        }

        private void UpdateWatchedTime(IProgress<Exception> progress, string watchedSubReddit)
        {
            try
            {
                TimeSpan watchedTime = _watchedSubRedditRepository.UpdateSubRedditWatchedTime(watchedSubReddit);
                if (watchedTime > _subRedditWatchTime)
                {
                    _timer.Enabled = false;
                    StopPolling();
                }
            }
            catch (Exception exception)
            {
                progress.Report(exception);
            }
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
                return;
            }
            _logger.LogInformation(@"Adding post [{FullName}] to watch list [{CurrentCount}]", 
                post.FullName, 
                _monitoredPostRepository.CountMonitoredPosts());
        }

        private bool NeedToPoll()
        {
            return _watchedSubRedditRepository.GetSubRedditWatchedTime(_pollerConfig.SubToWatch) < _subRedditWatchTime;
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
