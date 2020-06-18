using Common.Model.Document;

namespace Common.Model.Repositories
{
    public interface IMonitoredPostRepository
    {
        void Insert(IMonitoredPost monitoredPost);
    }
}