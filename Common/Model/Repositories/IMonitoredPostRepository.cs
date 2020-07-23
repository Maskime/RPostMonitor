using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Common.Model.Document;
using Common.Reddit;

namespace Common.Model.Repositories
{
    public interface IMonitoredPostRepository
    {
        bool Insert(IRedditPost redditMonitoredPost);
        List<IRedditMonitoredPost> FindPostToUpdate(int lastFetchOlderThanInSeconds,
            int maxSimultaneousFetch, TimeSpan maxPostAgeInDays);

        long CountMonitoredPosts();

        void AddVersion(IRedditPost newVersion);

        void SetFetching(string fullName, bool isFetching);

        IRedditMonitoredPost Get(string fullName);
        
        void SetFetchingAll(bool isFetching);

        List<IRedditMonitoredPost> FindAllPostAndVersions();
    }
}