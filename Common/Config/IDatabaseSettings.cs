namespace Common.Config
{
    public interface IDatabaseSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string MonitoredPostsCollectionName { get; set; }
    }
}