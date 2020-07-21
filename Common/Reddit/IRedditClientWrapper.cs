using System;
using System.Threading.Tasks;

using Common.Errors;

namespace Common.Reddit
{
    public interface IRedditClientWrapper
    {
        Task ListenToNewPosts(string sub, Action<IRedditPost> onNext, Action<PostMonitorException> onError);
        Task<IRedditPost> FetchAsync(string fullName);

        void StopListeningToNewPost(string watchedSub);

        public Action<bool> ConnectivityUpdated { get; set; }
    }
}