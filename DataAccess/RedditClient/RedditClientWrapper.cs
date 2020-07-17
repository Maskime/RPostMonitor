using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Common.Reddit;

using DataAccess.Config;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RedditSharp;
using RedditSharp.Things;

namespace DataAccess.RedditClient
{
    public class RedditClientWrapper : IRedditClientWrapper
    {
        private readonly Reddit _reddit;

        private readonly Dictionary<string, CancellationTokenSource> _subTokenSources =
            new Dictionary<string, CancellationTokenSource>();

        private readonly ILogger<RedditClientWrapper> _logger;
        private IMapper _mapper;
        private readonly ISet<string> _fetching = new HashSet<string>();

        public RedditClientWrapper(
            IOptions<RedditConfiguration> redditConfigOption
            , ILogger<RedditClientWrapper> logger
            , IMapper mapper
        )
        {
            var redditConfig = redditConfigOption.Value;
            _logger = logger;
            _mapper = mapper;
            _logger.LogDebug("Creating web agent");
            var webAgent = new BotWebAgent(redditConfig.Username, redditConfig.UserPassword, redditConfig.ClientId,
                redditConfig.ClientSecret, redditConfig.RedirectURI);
            //This will check if the access token is about to expire before each request and automatically request a new one for you
            //"false" means that it will NOT load the logged in user profile so reddit.User will be null
            _logger.LogDebug("Web agent created");
            _reddit = new Reddit(webAgent, false);
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

                    newPostHandler?.Invoke(_mapper.Map<RedditFetchedPost>(post));
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
        private readonly object _fetchingLock = new object();

        public bool Fetch(string fullName, out IRedditPost fetched)
        {
            try
            {
                lock (_fetchingLock)
                {
                    if (_fetching.Contains(fullName))
                    {
                        fetched = null;
                        _logger.LogDebug($"[{fullName}] is already being fetch, ignoring this one");
                        return false;
                    }
                }

                lock (_fetchingLock)
                {
                    _fetching.Add(fullName);
                }
                if (_reddit.GetThingByFullname(fullName) is Post post)
                {
                    fetched = _mapper.Map<IRedditPost>(post);
                    lock (_fetchingLock)
                    {
                        _fetching.Remove(post.FullName);
                    }
                    return true;
                }
                _logger.LogWarning($"Could not retrieve updated information for [{fullName}]");
                fetched = null;
                return false;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"When fetching updated info for [{fullName}]");
                throw;
            }
        }

        public Task<IRedditPost> FetchAsync(string fullName)
        {
            return Task.Run(() =>
            {
                if (_reddit.GetThingByFullname(fullName) is Post post)
                {
                    return (IRedditPost)_mapper.Map<RedditFetchedPost>(post);
                }
                return null;
            });
            
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
