namespace PostMonitor.Config
{
    public class UpdaterConfiguration
    {
        public const string ConfigKey = "PostUpdater";

        public long Periodicity { get; set; }
        public long WatchDuration { get; set; }
        public long InactivityTimeout { get; set; }
    }
}
