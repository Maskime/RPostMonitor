using Common.Reddit;

namespace Common.Model.Document
{
    public interface IMonitoredPost:IRedditPost
    {
        int IterationsNumber { get; set; }
    }
}