// ************************************************************************************************
// 
//  © 2019       General Electric Company
// 
//  Description  See class summary below.
// 
//  History      See source code control system.
// 
// ************************************************************************************************

using DataAccess.Config;
using DataAccess.Documents;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MongoDB.Driver;

namespace DataAccess
{
    public class DatabaseContext:IDatabaseContext
    {
        private readonly IMongoCollection<RedditMonitoredPostDocument> _monitoredPosts;
        private readonly IMongoCollection<RedditMonitoredPostVersionDocument> _monitoredPostVersions;
        private readonly IMongoCollection<WatchedSubRedditDocument> _watchedSubReddits;
        private ILogger<DatabaseContext> _logger;

        public IMongoCollection<RedditMonitoredPostDocument> MonitoredPosts => _monitoredPosts;
        public IMongoCollection<RedditMonitoredPostVersionDocument> MonitoredPostVersions => _monitoredPostVersions;
        public IMongoCollection<WatchedSubRedditDocument> WatchedSubReddits => _watchedSubReddits;

        public DatabaseContext(
            IOptions<DatabaseSettings> settings
            , ILogger<DatabaseContext> logger
            )
        {
            _logger = logger;
            
            DatabaseSettings settingsValue = settings.Value;
            _logger.LogDebug("Initializing DB connection");
            var client = new MongoClient(settingsValue.ConnectionString);

            var database = client.GetDatabase(settingsValue.DatabaseName);

            _monitoredPosts = 
                database.GetCollection<RedditMonitoredPostDocument>(settingsValue.MonitoredPostsCollectionName);
            _monitoredPostVersions =
                database.GetCollection<RedditMonitoredPostVersionDocument>(settingsValue.MonitoredPostVersionsCollectionName);
            _watchedSubReddits =
                database.GetCollection<WatchedSubRedditDocument>(settingsValue.WatchedSubRedditCollectionName);
            _logger.LogDebug("Database collections initialized");
        }
    }
}
