using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Common.Reddit;

using DataAccess.Config;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RedditSharp;
using RedditSharp.Things;

namespace DataAccess.Reddit
{
    public class RedditWrapper : IRedditWrapper
    {
        private readonly RedditSharp.Reddit _reddit;

        private readonly Dictionary<string, CancellationTokenSource> _subTokenSources =
            new Dictionary<string, CancellationTokenSource>();

        private readonly ILogger<RedditWrapper> _logger;

        public RedditWrapper(
            IOptions<RedditConfiguration> redditConfigOption
            , ILogger<RedditWrapper> logger
        )
        {
            var redditConfig = redditConfigOption.Value;

            var webAgent = new BotWebAgent(redditConfig.Username, redditConfig.UserPassword, redditConfig.ClientId,
                redditConfig.ClientSecret, redditConfig.RedirectURI);
            //This will check if the access token is about to expire before each request and automatically request a new one for you
            //"false" means that it will NOT load the logged in user profile so reddit.User will be null
            _reddit = new RedditSharp.Reddit(webAgent, false);
            _logger = logger;
        }

        public async Task ListenToNewPosts(string sub, Action<IRedditPost> newPostHandler)
        {
            if (_subTokenSources.ContainsKey(sub))
            {
                _logger.LogWarning($"There already is a listener on this sub [{sub}], not creating a second one...");
                return;
            }

            _logger.LogInformation($"Creating a new listener for {sub}");
            Subreddit subreddit = await _reddit.GetSubredditAsync(sub);
            var currentCancellationSource = new CancellationTokenSource();
            _subTokenSources.Add(sub, currentCancellationSource);
            CancellationToken cancellationToken = currentCancellationSource.Token;
            var pollingTask = Task.Run(() =>
            {
                foreach (Post post in subreddit.New.GetListingStream())
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        break;
                    }

                    newPostHandler?.Invoke(RedditPost.From(post));
                }
            }, cancellationToken);

            try
            {
                await pollingTask;
            }
            catch (OperationCanceledException exception)
            {
                _logger.LogInformation("Operation canceled");
            }
        }

        public IRedditPost Fetch(string permalink)
        {
            try
            {
                Post post = _reddit.GetPost(new Uri($"https://www.reddit.com/{permalink}"));
                if (post != null)
                {
                    return RedditPost.From(post);
                }
                _logger.LogWarning($"Could not retrieve updated information for [{permalink}]");
                return null;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"When fetching updated info for [{permalink}]");
                throw;
            }

            
        }

        public void StopListeningToNewPost(string watchedSub)
        {
            if (!_subTokenSources.ContainsKey(watchedSub))
            {
                _logger.LogWarning($"You tried to stop a listener that does not exists [{watchedSub}]");
                return;
            }

            CancellationTokenSource tokenSource = _subTokenSources[watchedSub];
            tokenSource.Cancel();
            tokenSource.Dispose();
            _subTokenSources.Remove(watchedSub);
        }
    }
}
