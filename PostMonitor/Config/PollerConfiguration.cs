namespace PostMonitor.Config
{
    public class PollerConfiguration
    {
        public const string ConfigKey = "Poller";
        
        public string Name { get; set; }

        public string DownloadDir { get; set; }
    }
}