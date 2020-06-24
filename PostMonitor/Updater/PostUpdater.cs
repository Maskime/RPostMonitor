﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Common.Model.Repositories;
using Common.Reddit;

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

        public PostUpdater(ILogger<PostUpdater> logger
            ,IOptions<UpdaterConfiguration> configOption
            , IMonitoredPostRepository repo
            , IRedditWrapper wrapper
        )
        {
            _logger = logger;
            _config = configOption.Value;
            _repo = repo;
            _wrapper = wrapper;
        }

        private void UpdatePosts(object sender)
        {
            var postToUpdate = _repo.FindPostWithLastFetchedOlderThan(60);
            if (postToUpdate == null)
            {
                return;
            }

            foreach (var monitoredPost in postToUpdate)
            {
                var redditPost = _wrapper.Fetch(monitoredPost.Permalink);
                _logger.LogDebug($"Fetched [{redditPost.Title}] [{redditPost.RedditId}]");
            }
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