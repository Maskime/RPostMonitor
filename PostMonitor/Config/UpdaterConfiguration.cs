namespace PostMonitor.Config
{
    public class UpdaterConfiguration
    {
        public const string ConfigKey = "PostUpdater";

        public long PeriodicityInSeconds { get; set; }
        public long WatchDurationInDays { get; set; }
        public long InactivityTimeoutInHours { get; set; }
        public int SimultaneousFetchRequest { get; set; }
    }
}
