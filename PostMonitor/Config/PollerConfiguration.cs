namespace PostMonitor.Config
{
    public class PollerConfiguration
    {
        /// <summary>
        /// Key for this configuration in the appsettings.json file.
        /// </summary>
        public const string ConfigKey = "Poller";

        public DownloadConfiguration Download { get; set; }

        /// <summary>
        /// SubReddit to watch.
        /// </summary>
        public string  SubToWatch { get; set; }

        /// <summary>
        /// If a new post is older than that, we don't add it to the watch list.
        /// </summary>
        public double NewPostMaxAgeInMinutes { get; set; }

        /// <summary>
        /// How long should SubReddits watched for.
        /// </summary>
        public double SubRedditWatchTimeInHours { get; set; }
    }
}