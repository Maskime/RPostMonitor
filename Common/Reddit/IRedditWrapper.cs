using System;
using System.Threading.Tasks;

namespace Common.Reddit
{
    public interface IRedditWrapper
    {
        Task ListenToNewPosts(string sub, Action<IRedditPost> newPostHandler);
        IRedditPost Fetch(string permalink);
    }
}