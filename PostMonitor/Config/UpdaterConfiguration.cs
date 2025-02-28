namespace PostMonitor.Config
{
    public class UpdaterConfiguration
    {
        /// <summary>
        /// Name of the node in the config file.
        /// </summary>
        public const string ConfigKey = "PostUpdater";

        /// <summary>
        /// How often should we check for posts to update in the DB
        /// </summary>
        public long PeriodicityInSeconds { get; set; }

        /// <summary>
        /// How long should we watch a post
        /// Refer to https://docs.microsoft.com/en-us/dotnet/api/system.timespan.parse?view=netcore-3.1
        /// </summary>
        public string MaxPostAge { get; set; }
        
        /// <summary>
        /// When should we try to update a post from reddit.
        /// </summary>
        public int TimeBetweenFetchInSeconds { get; set; }
        
        /// <summary>
        /// How many update http requests we should send at the same time.
        /// </summary>
        public int SimultaneousFetchRequest { get; set; }
    }
}
