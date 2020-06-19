using System;
using System.Threading.Tasks;
using Common.Reddit;
using Microsoft.Extensions.Options;
using RedditSharp;

namespace DataAccess.Reddit
{
    public class RedditWrapper:IRedditWrapper
    {
        private readonly RedditSharp.Reddit _reddit;

        public RedditWrapper(IOptions<RedditConfiguration> redditConfigOption)
        {
            var redditConfig = redditConfigOption.Value;
            
            var webAgent = new BotWebAgent(redditConfig.Username, redditConfig.UserPassword, redditConfig.ClientId,
                redditConfig.ClientSecret, redditConfig.RedirectURI);
            //This will check if the access token is about to expire before each request and automatically request a new one for you
            //"false" means that it will NOT load the logged in user profile so reddit.User will be null
            _reddit = new RedditSharp.Reddit(webAgent, false);
        }

        public async Task ListenToNewPosts(string sub, Action<IRedditPost> newPostHandler)
        {
            var subreddit = await _reddit.GetSubredditAsync(sub);
            await Task.Run(() =>
                {
                    foreach (var post in subreddit.New.GetListingStream())
                    {
                        newPostHandler?.Invoke(RedditPost.From(post));
                    }
                });
        }

        public IRedditPost Fetch(string permalink)
        {
            return RedditPost.From(_reddit.GetPost(new Uri(permalink)));
        }
    }
}