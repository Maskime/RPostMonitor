using System;

using Common.Model.Document;

namespace DataAccess.Documents
{
    public class RedditMonitoredPostDocument:RedditPostDocument,IRedditMonitoredPost
    {
        public int IterationNumber { get; set; }
        public TimeSpan InactivityAge { get; set; }
        public TimeSpan Age { get; set; }
    }
}