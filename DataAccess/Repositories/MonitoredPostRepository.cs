using System;
using System.Collections.Generic;

using Common.Model.Document;
using Common.Model.Repositories;

using DataAccess.Config;
using DataAccess.Documents;

using Microsoft.Extensions.Options;

using MongoDB.Driver;

namespace DataAccess.Repositories
{
    public class MonitoredPostRepository:IMonitoredPostRepository
    {
        private IMongoCollection<MonitoredPost> _posts;
        private DatabaseSettings _settings;

        public MonitoredPostRepository(IOptions<DatabaseSettings> settings)
        {
            _settings = settings.Value;
            var client = new MongoClient(_settings.ConnectionString);

            var database = client.GetDatabase(_settings.DatabaseName);

            _posts = database.GetCollection<MonitoredPost>(_settings.MonitoredPostsCollectionName);
        }

        public void Insert(IMonitoredPost monitoredPost)
        {
            _posts.InsertOne(new MonitoredPost
            {
                Author = monitoredPost.Author,
                InsertedAt = DateTime.Now,
                CreatedAt = monitoredPost.CreatedAt,
                FetchedAt = monitoredPost.FetchedAt,
                Permalink = monitoredPost.Permalink,
                RedditId = monitoredPost.RedditId,
                Url = monitoredPost.Url,
                Title = monitoredPost.Title
            });
        }

        public List<IMonitoredPost> FindPostWithLastFetchedOlderThan(int nbSeconds)
        {
            var dateTimeOffset = DateTimeOffset.Now;
            var maxLastFetch = dateTimeOffset.AddSeconds(nbSeconds * -1);
            
            return new List<IMonitoredPost>(_posts
                .Find(post =>
                    post.FetchedAt <= maxLastFetch
                )
                .ToList());
        }

        public long CountMonitoredPosts()
        {
            return _posts.CountDocuments(p => true);
        }
    }
}