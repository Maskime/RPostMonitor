using System;
using System.Threading.Tasks;

namespace Common.Reddit
{
    public interface IRedditWrapper
    {
        Task ListenToNewPosts(string sub, Action<IRedditPost> newPostHandler);
        bool Fetch(string fullName, out IRedditPost fetchedPost);
        Task<IRedditPost> FetchAsync(string fullName);

        void StopListeningToNewPost(string watchedSub);
    }
}