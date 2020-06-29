using System;
using System.Collections.Generic;
using System.Linq;

using AutoMapper;

using Common.Errors;
using Common.Model.Document;
using Common.Model.Repositories;

using DataAccess.Config;
using DataAccess.Documents;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MongoDB.Bson;
using MongoDB.Driver;

namespace DataAccess.Repositories
{
    public class MonitoredPostRepository:IMonitoredPostRepository
    {
        private IMongoCollection<MonitoredPost> _posts;
        private DatabaseSettings _settings;
        private IMapper _mapper;
        private ILogger<MonitoredPostRepository> _logger;
        private IMongoCollection<MonitoredPost> _postVersions;

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

            _posts = database.GetCollection<MonitoredPost>(_settings.MonitoredPostsCollectionName);
            _postVersions = database.GetCollection<MonitoredPost>(_settings.MonitoredPostVersionsCollectionName);
        }

        public bool Insert(IMonitoredPost monitoredPost)
        {
            long countDocuments = _posts.CountDocuments(p => p.RedditId.Equals(monitoredPost.Id));
            if (countDocuments > 0)
            {
                _logger.LogWarning($"Post with id [{monitoredPost.Id}] already watched, skipping");
                return false;
            }

            var document = _mapper.Map<MonitoredPost>(monitoredPost);
            document.IterationsNumber = 1;
            _posts.InsertOne(document);
            return true;
        }

        public List<IMonitoredPost> FindPostWithLastFetchedOlderThan(int nbSeconds)
        {
            var dateTimeOffset = DateTimeOffset.Now;
            var maxLastFetch = dateTimeOffset.AddSeconds(nbSeconds * -1);

            var sort = Builders<MonitoredPost>.Sort
                                              .Ascending(p => p.IterationsNumber)
                                              .Descending(p => p.FetchedAt);
            return new List<IMonitoredPost>(_posts
                .Find(post =>
                    post.FetchedAt <= maxLastFetch 
                    && !post.SelfTextHtml.Contains("SC_OFF") //Marked as deleted
                    && !post.CurrentlyFetched // A flag to avoid to retrieve post that are already being fetched
                )
                .Sort(sort)
                .ToList()
                .Select(p => _mapper.Map<IMonitoredPost>(p)));
        }

        public long CountMonitoredPosts()
        {
            return _posts.CountDocuments(p => true);
        }

        private MonitoredPost Get(string redditId)
        {
            return _posts.Find(p => p.RedditId == redditId).FirstOrDefault();
        }

        public void AddVersion(IMonitoredPost newVersion)
        {
            var newPostVersion = _mapper.Map<MonitoredPost>(newVersion);
            MonitoredPost oldVersion = Get(newPostVersion.RedditId);
            if (oldVersion == null)
            {
                throw new PostMonitorException($"Could not find original version for post id [{newVersion.Id}]");
            }
            newPostVersion.IterationsNumber = oldVersion.IterationsNumber + 1;

            newPostVersion.Id = oldVersion.Id;
            _posts.ReplaceOne(p => p.RedditId == newPostVersion.RedditId, newPostVersion);

            var versionedPost = _mapper.Map<MonitoredPost>(oldVersion);
            versionedPost.Id = null;
            _postVersions.InsertOne(versionedPost);
        }

        public void SetFetching(string monitoredPostId, bool currentlyFetched)
        {
            var update = Builders<MonitoredPost>.Update.Set(p => p.CurrentlyFetched, currentlyFetched);
            _posts.UpdateOne(p=> p.RedditId == monitoredPostId, update);
        }
    }
}