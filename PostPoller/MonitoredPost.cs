using System;
using Common.Model.Document;

namespace PostPoller
{
    public class MonitoredPost:IMonitoredPost
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime FetchedAt { get; set; }
        public string RedditId { get; set; }
        public string Permalink { get; set; }
        public string Url { get; set; }
    }
}