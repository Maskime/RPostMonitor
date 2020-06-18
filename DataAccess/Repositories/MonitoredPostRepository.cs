using System;
using Common.Model.Document;
using Common.Model.Repositories;
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
                CreatedAt = DateTime.Now,
                FetchedAt = monitoredPost.FetchedAt,
                Permalink = monitoredPost.Permalink,
                RedditId = monitoredPost.RedditId,
                Url = monitoredPost.Url
            });
        }
    }
}