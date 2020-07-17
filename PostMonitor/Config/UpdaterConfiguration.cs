namespace PostMonitor.Config
{
    public class UpdaterConfiguration
    {
        /// <summary>
        /// Name of the node in the config file.
        /// </summary>
        public const string ConfigKey = "PostUpdater";

        /// <summary>
        /// How often should we check for an update of posts on reddit
        /// </summary>
        public long PeriodicityInSeconds { get; set; }
        /// <summary>
        /// How many iteration should we do on each post.
        /// </summary>
        public int NbIterationOnPost { get; set; }
        /// <summary>
        /// When should we try to update a post from reddit.
        /// </summary>
        public int TimeBetweenFetchInSeconds { get; set; }
        /// <summary>
        /// If the post didn't have any change in this span, we stop trying to update it.
        /// </summary>
        public long InactivityTimeoutInHours { get; set; }
        /// <summary>
        /// How many update http requests we should send at the same time.
        /// </summary>
        public int SimultaneousFetchRequest { get; set; }
    }
}
