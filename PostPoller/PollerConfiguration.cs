namespace PostPoller
{
    public class PollerConfiguration
    {
        public const string ConfigKey = "Poller";
        
        public string Name { get; set; }
        public RedditConfiguration Reddit { get; set; }
        
        public string DownloadDir { get; set; }
    }
}