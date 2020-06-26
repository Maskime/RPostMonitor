using System.Collections.Generic;

using Common.Model.Document;

namespace Common.Model.Repositories
{
    public interface IMonitoredPostRepository
    {
        bool Insert(IMonitoredPost monitoredPost);
        List<IMonitoredPost> FindPostWithLastFetchedOlderThan(int nbSeconds);

        long CountMonitoredPosts();

        void AddVersion(IMonitoredPost currentVersion, IMonitoredPost newVersion);
    }
}