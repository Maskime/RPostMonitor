namespace PostMonitor.Config
{
    public class PollerConfiguration
    {
        public const string ConfigKey = "Poller";
        
        public string DownloadDir { get; set; }

        public string  SubToWatch { get; set; }

    }
}