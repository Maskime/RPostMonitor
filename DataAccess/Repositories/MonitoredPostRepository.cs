using System;
using System.Collections.Generic;

using AutoMapper;

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
        private IMapper _mapper;

        public MonitoredPostRepository(IOptions<DatabaseSettings> settings, IMapper mapper)
        {
            _settings = settings.Value;
            _mapper = mapper;
            var client = new MongoClient(_settings.ConnectionString);

            var database = client.GetDatabase(_settings.DatabaseName);

            _posts = database.GetCollection<MonitoredPost>(_settings.MonitoredPostsCollectionName);
        }

        public void Insert(IMonitoredPost monitoredPost)
        {
            _posts.InsertOne(_mapper.Map<MonitoredPost>(monitoredPost));
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