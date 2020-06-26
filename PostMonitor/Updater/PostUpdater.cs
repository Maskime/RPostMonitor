using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Common.Model.Document;
using Common.Model.Repositories;
using Common.Reddit;

using DataAccess.Documents;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PostMonitor.Config;

namespace PostMonitor.Updater
{
    public class PostUpdater:IHostedService, IDisposable
    {
        private readonly UpdaterConfiguration _config;
        private readonly IMonitoredPostRepository _repo;
        private readonly ILogger<PostUpdater> _logger;
        private Timer _timer;
        private readonly IRedditWrapper _wrapper;
        private IMapper _mapper;
        private bool _updateRunning;

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

        private void UpdatePosts(object sender)
        {
            if (_updateRunning)
            {
                _logger.LogDebug("There already an update operation on going skipping this one.");
                return;
            }

            _updateRunning = true;
            List<IMonitoredPost> postToUpdate = _repo.FindPostWithLastFetchedOlderThan(60);
            if (postToUpdate == null)
            {
                return;
            }

            foreach (IMonitoredPost monitoredPost in postToUpdate)
            {
                if (!_wrapper.Fetch(monitoredPost.FullName, out IRedditPost update))
                {
                    continue;
                }
                _repo.AddVersion(monitoredPost, _mapper.Map<IMonitoredPost>(update));
                _logger.LogDebug($"Fetched [{update.Title}] [{update.Id}][{monitoredPost.IterationsNumber}]");
            }

            _updateRunning = false;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(UpdatePosts, null, TimeSpan.Zero, TimeSpan.FromSeconds(_config.Periodicity));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}