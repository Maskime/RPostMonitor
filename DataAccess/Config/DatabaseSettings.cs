namespace DataAccess.Config
{
    public class DatabaseSettings
    {
        public const string ConfigKey = "DatabaseSettings";
        
        public string MonitoredPostsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string MonitoredPostVersionsCollectionName { get; set; }
    }
}