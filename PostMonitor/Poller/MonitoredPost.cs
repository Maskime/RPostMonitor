using System;

using Common.Model.Document;

namespace PostMonitor.Poller
{
    public class MonitoredPost:IMonitoredPost
    {
        public string Author { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset FetchedAt { get; set; }
        public string RedditId { get; set; }
        public string Permalink { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
    }
}