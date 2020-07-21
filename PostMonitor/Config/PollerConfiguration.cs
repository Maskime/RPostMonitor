namespace PostMonitor.Config
{
    public class PollerConfiguration
    {
        public const string ConfigKey = "Poller";

        public DownloadConfiguration Download { get; set; }

        public string  SubToWatch { get; set; }

        public long NbPostToMonitor { get; set; }
        public double NewPostMaxAgeInMinutes { get; set; }
    }
}