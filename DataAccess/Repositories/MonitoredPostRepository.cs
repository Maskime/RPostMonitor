using Common.Config;
using Common.Model.Document;
using Common.Model.Repositories;
using DataAccess.Model;
using MongoDB.Driver;

namespace DataAccess.Repositories
{
    public class MonitoredPostRepository:IMonitoredPostRepository
    {
        private IMongoCollection<MonitoredPost> _posts;

        public MonitoredPostRepository(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _posts = database.GetCollection<MonitoredPost>(settings.MonitoredPostsCollectionName);
        }

        public void Insert(IMonitoredPost monitoredPost)
        {
            _posts.InsertOne((MonitoredPost) monitoredPost);
        }
    }
}