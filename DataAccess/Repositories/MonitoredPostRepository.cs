using System;
using System.Collections.Generic;
using System.Linq;

using AutoMapper;

using Common.Errors;
using Common.Model.Document;
using Common.Model.Repositories;
using Common.Reddit;

using DataAccess.Config;
using DataAccess.Documents;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MongoDB.Driver;

namespace DataAccess.Repositories
{
    public class MonitoredPostRepository : IMonitoredPostRepository
    {
        private IMongoCollection<RedditMonitoredPostDocument> _posts;
        private DatabaseSettings _settings;
        private IMapper _mapper;
        private ILogger<MonitoredPostRepository> _logger;
        private IMongoCollection<RedditMonitoredPostDocument> _postVersions;

        public MonitoredPostRepository(
            IOptions<DatabaseSettings> settings
            , IMapper mapper
            , ILogger<MonitoredPostRepository> logger
        )
        {
            _settings = settings.Value;
            _mapper = mapper;
            _logger = logger;
            var client = new MongoClient(_settings.ConnectionString);

            var database = client.GetDatabase(_settings.DatabaseName);

            _posts = database.GetCollection<RedditMonitoredPostDocument>(_settings.MonitoredPostsCollectionName);
            _postVersions = database.GetCollection<RedditMonitoredPostDocument>(_settings.MonitoredPostVersionsCollectionName);
        }

        public bool Insert(IRedditPost redditPost)
        {
            long countDocuments = _posts.CountDocuments(p => p.FullName.Equals(redditPost.FullName));
            if (countDocuments > 0)
            {
                _logger.LogWarning("Post with FullName [{}] already watched, skipping", redditPost.FullName);
                return false;
            }

            var document = _mapper.Map<RedditMonitoredPostDocument>(redditPost);
            document.IterationNumber = 1;
            _posts.InsertOne(document);
            return true;
        }

        public List<IRedditMonitoredPost> FindPostToUpdate(
            int lastFetchOlderThanInSeconds, 
            int maxNumberOfIterations,
            long configInactivityTimeoutInHours,
            int maxSimultaneousFetch)
        {
            _logger.LogDebug(@"Entering FindPostToUpdate");
            var dateTimeOffset = DateTimeOffset.Now;
            var maxLastFetch = dateTimeOffset.AddSeconds(lastFetchOlderThanInSeconds * -1);

            var sort = Builders<RedditMonitoredPostDocument>.Sort
                                                            .Ascending(p =>
                                                                p.IterationNumber); // Give the priority to the posts that where less updated.
            try
            {
                var toUpdate = new List<IRedditMonitoredPost>(_posts
                                                              .Find(post =>
                                                                      post.FetchedAt <= maxLastFetch
                                                                      && !post.SelfTextHtml.Contains("SC_OFF") //Marked as deleted
                                                                      && !post.IsFetching // A flag to avoid to retrieve posts that are currently being fetched
                                                                      && post.IterationNumber < maxNumberOfIterations // Max number of times to fetch the post.
                                                                      && post.InactivityAge < TimeSpan.FromHours(configInactivityTimeoutInHours) // Post that have been inactive for too long should not be fetched anymore.
                                                              )
                                                              .Sort(sort)
                                                              .Limit(maxSimultaneousFetch)
                                                              .ToList())
                    ;
                _logger.LogDebug(@"Found [{}] post to update", toUpdate.Count);
                return toUpdate;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, @"Error when fetching post to update in DB");
                throw;
            }
        }

        public long CountMonitoredPosts()
        {
            return _posts.CountDocuments(p => true);
        }

        public IRedditMonitoredPost Get(string fullName)
        {
            return _posts.Find(p => p.FullName == fullName).FirstOrDefault();
        }

        public void AddVersion(IRedditPost newVersion)
        {
            var newPostVersion = _mapper.Map<RedditMonitoredPostDocument>(newVersion);
            if (!(Get(newPostVersion.FullName) is RedditMonitoredPostDocument oldVersion))
            {
                throw new PostMonitorException($"Could not find original version for post id [{newVersion.Id}]");
            }

            newPostVersion.IterationNumber = oldVersion.IterationNumber + 1;
            newPostVersion.InactivityAge = TimeSpan.Zero;

            newPostVersion.Id = oldVersion.Id;
            _posts.ReplaceOne(p => p.FullName == newPostVersion.FullName, newPostVersion);

            oldVersion.Id = null;
            _postVersions.InsertOne(oldVersion);
        }

        public void SetFetching(string fullName, bool isFetching)
        {
            var update = Builders<RedditMonitoredPostDocument>.Update.Set(p => p.IsFetching, isFetching);
            _posts.UpdateOne(p => p.FullName == fullName, update);
        }

        public long CountPostWithMissingIterations(int configNbIterationOnPost)
        {
            return _posts.CountDocuments(p =>
                    p.IterationNumber < configNbIterationOnPost
                    && !p.SelfTextHtml.Contains("SC_OFF") //Marked as deleted
            );
        }

        public void UpdatePostInactivity(IRedditMonitoredPost lastVersion)
        {
            TimeSpan inactivityAge = lastVersion.InactivityAge + (DateTimeOffset.Now - lastVersion.FetchedAt);
            UpdateDefinition<RedditMonitoredPostDocument> update = Builders<RedditMonitoredPostDocument>
                                                                   .Update
                                                                   .Set(p => p.InactivityAge, inactivityAge)
                                                                   .Set(p => p.FetchedAt, DateTimeOffset.Now);
            _posts.UpdateOne(p => p.FullName == lastVersion.FullName, update);
        }

        public void SetFetchingAll(bool isFetching)
        {
            var update = Builders<RedditMonitoredPostDocument>.Update.Set(p => p.IsFetching, isFetching);
            _posts.UpdateMany(p => true, update);
        }
    }
}
