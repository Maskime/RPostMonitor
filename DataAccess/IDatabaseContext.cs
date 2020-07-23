// ************************************************************************************************
// 
//  © 2019       General Electric Company
// 
//  Description  See class summary below.
// 
//  History      See source code control system.
// 
// ************************************************************************************************

using DataAccess.Documents;

using MongoDB.Driver;

namespace DataAccess
{
    public interface IDatabaseContext
    {
        IMongoCollection<RedditMonitoredPostDocument> MonitoredPosts { get; }

        IMongoCollection<RedditMonitoredPostVersionDocument> MonitoredPostVersions { get; }

        IMongoCollection<WatchedSubRedditDocument> WatchedSubReddits { get; }

    }
}
