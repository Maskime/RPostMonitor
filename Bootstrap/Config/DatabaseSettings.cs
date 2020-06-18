using Common.Config;

namespace PostPoller.Config
{
    public class DatabaseSettings:IDatabaseSettings
    {
        public string MonitoredPostsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}