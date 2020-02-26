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
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RedditSharp;
using RedditSharp.Things;

namespace RedditClientTest
{
    public class RGoddessesPoller : IHostedService
    {
        private readonly ILogger<RGoddessesPoller> logger;
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly RefreshTokenWebAgentPool agentPool;
        private readonly ApplicationConfiguration appConfig;

        public RGoddessesPoller(
            ILogger<RGoddessesPoller> logger,
            IHostApplicationLifetime applicationLifetime,
            IOptions<ApplicationConfiguration> appConfig,
            RefreshTokenWebAgentPool agentPool)
        {
            this.logger = logger;
            this.applicationLifetime = applicationLifetime;
            this.agentPool = agentPool;
            this.appConfig = appConfig.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug(@"StartAsync called");
            applicationLifetime.ApplicationStarted.Register(OnStarted);
            applicationLifetime.ApplicationStopped.Register(OnStopped);
            applicationLifetime.ApplicationStopping.Register(OnStopping);
            logger.LogDebug($"Configuration values [{appConfig.Name}]");
            StartPolling(cancellationToken);
            return Task.CompletedTask;
        }

        private async Task StartPolling(CancellationToken cancellationToken)
        {
            RedditConfiguration redditConfig = appConfig.Reddit;
            var webAgent = new BotWebAgent(redditConfig.Username, "***REMOVED***", redditConfig.ClientId,
                redditConfig.ClientSecret, redditConfig.RedirectURI);
            //This will check if the access token is about to expire before each request and automatically request a new one for you
            //"false" means that it will NOT load the logged in user profile so reddit.User will be null
            var reddit = new Reddit(webAgent, false);
            Subreddit subreddit = await reddit.GetSubredditAsync("/r/AskReddit");
            logger.LogDebug("Creating stream");
            ListingStream<Post> postStream = subreddit.GetPosts(Subreddit.Sort.New)
                                                         .Stream();
            logger.LogDebug("Subscription");
            postStream.Subscribe(HandlingPost);
            logger.LogDebug("ENumeration");
            await postStream.Enumerate(cancellationToken);
            // while (postStream.GetEnumerator().MoveNext())
            // {
            //     HandlingPost(postStream.GetEnumerator().Current);
            // }
            // logger.LogDebug("outside while loop");
        }

        private void OnCompleted()
        {
            throw new NotImplementedException();
        }

        private void OnError(Exception obj)
        {
            throw new NotImplementedException();
        }

        private void HandlingPost(Post post)
        {
            logger.LogDebug($"Post : [{post.Title} at {post.CreatedUTC}]");
        }

        private void OnStopping()
        {
            logger.LogInformation($"Stopping Poller");
        }

        private void OnStopped()
        {
            logger.LogInformation($"Poller Stopped");
        }

        private void OnStarted()
        {
            logger.LogInformation($"Poller Started");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug(@"StopAsync called");
            return Task.CompletedTask;
        }
    }
}
