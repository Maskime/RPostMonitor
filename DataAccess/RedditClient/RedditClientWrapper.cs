using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using Common.Errors;
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
        private Reddit _reddit;

        private readonly Dictionary<string, CancellationTokenSource> _subTokenSources =
            new Dictionary<string, CancellationTokenSource>();

        private readonly ILogger<RedditClientWrapper> _logger;
        private IMapper _mapper;
        private readonly RedditConfiguration _config;

        private bool _connectivityStatus;
        private Action<bool> _connectivityUpdated;
        private bool _reconnectInProgress;
        private string _userAgent;

        public Action<bool> ConnectivityUpdated
        {
            get => _connectivityUpdated;
            set
            {
                // When there is a new listener, we give it the connectivity status right away to avoid that
                // it miss the current status
                _connectivityUpdated = value;
                _connectivityUpdated?.Invoke(_connectivityStatus);
            }
        }

        public RedditClientWrapper(
            IOptions<RedditConfiguration> redditConfigOption
            , ILogger<RedditClientWrapper> logger
            , IMapper mapper
        )
        {
            _config = redditConfigOption.Value;
            _logger = logger;
            _mapper = mapper;
            _userAgent = $"RPMonitor (by {_config.Username})";
            try
            {
                ReconnectAgent();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not create webagent");
                UpdateConnectivity(false);
            }
        }

        private void UpdateConnectivity(bool connectivityStatus)
        {
            _connectivityStatus = connectivityStatus;
            ConnectivityUpdated?.Invoke(_connectivityStatus);
            if (!connectivityStatus)
            {
                ReconnectAgent();
            }
        }

        private void ReconnectAgent()
        {
            if (_reconnectInProgress)
            {
                _logger.LogDebug("There already is a connection attempt in progress");
                return;
            }
            _reconnectInProgress = true;
            while (!_connectivityStatus)
            {
                var retryCount = 0;
                for (retryCount = 0; retryCount < _config.MaxRetry; retryCount++)
                {
                    try
                    {
                        _logger.LogDebug("Creating web agent");
                        var webAgent = new BotWebAgent(_config.Username, _config.UserPassword, _config.ClientId,
                            _config.ClientSecret, _config.RedirectURI)
                        {
                            UserAgent = _userAgent
                        };
                        //This will check if the access token is about to expire before each request and automatically request a new one for you
                        //"false" means that it will NOT load the logged in user profile so reddit.User will be null
                        _logger.LogDebug("Web agent created");
                        _reddit = new Reddit(webAgent, false);
                        UpdateConnectivity(true);
                        _reconnectInProgress = false;
                        break;
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Cannot connect webagent");
                    }
                    _logger.LogDebug("Waiting for {WaitTime}s to avoid being seen as DDOS attempt", _config.TimeBetweenRetryAttemptInSeconds);
                    Thread.Sleep(TimeSpan.FromSeconds(_config.TimeBetweenRetryAttemptInSeconds));
                }

                if (retryCount == _config.MaxRetry)
                {
                    _logger.LogWarning("Reconnection failed after {MaxRetry} attempts, sleep for {SleepTimeInSeconds}s before retrying", 
                        _config.MaxRetry, _config.TimeBetweenRetrySequenceInSeconds);
                    Thread.Sleep(TimeSpan.FromSeconds(_config.TimeBetweenRetrySequenceInSeconds));
                }
            }
        }

        public async Task ListenToNewPosts(string sub, Action<IRedditPost> onNext, Action<PostMonitorException> onError)
        {
            if (_subTokenSources.ContainsKey(sub))
            {
                _logger.LogWarning("There already is a listener on this sub [{Subreddit}], not creating a second one...", sub);
                return;
            }

            _logger.LogInformation("Creating a new listener for {SubReddit}", sub);
            Subreddit subreddit = await _reddit.GetSubredditAsync(sub);
            var currentCancellationSource = new CancellationTokenSource();
            _subTokenSources.Add(sub, currentCancellationSource);
            CancellationToken cancellationToken = currentCancellationSource.Token;

            var stream = subreddit
                         .GetPosts(Subreddit.Sort.New)
                         .Stream();
            stream.Subscribe(
                    (post) => onNext(_mapper.Map<RedditFetchedPost>(post)),
                    (ex) => onError(new PostMonitorException("Error when fetching next post", ex)))
                ;

            try
            {
                await stream.Enumerate(cancellationToken);
            }
            catch (OperationCanceledException exception)
            {
                _logger.LogInformation("Operation canceled {@Exception}", exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connectivity lost on Listener");
                UpdateConnectivity(false);
            }
        }
        public async Task<IRedditPost> FetchAsync(string fullName)
        {
            try
            {
                Thing post = await _reddit.GetThingByFullnameAsync(fullName);
                return post is Post ? _mapper.Map<RedditFetchedPost>(post) : null;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Connectivity issue on FetctAsync");
                UpdateConnectivity(false);
                return null;
            }
        }

        public void StopListeningToNewPost(string watchedSub)
        {
            if (!_subTokenSources.ContainsKey(watchedSub))
            {
                _logger.LogWarning("You tried to stop a listener that does not exists [{SubReddit}]", watchedSub);
                return;
            }

            CancellationTokenSource tokenSource = _subTokenSources[watchedSub];
            tokenSource.Cancel();
            tokenSource.Dispose();
            _subTokenSources.Remove(watchedSub);
        }
    }
}
