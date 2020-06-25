using System;

using Common.Model.Document;

namespace PostMonitor.Poller
{
    public class MonitoredPost:IMonitoredPost
    {
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset CreatedUTC { get; set; }
        public DateTimeOffset FetchedAt { get; set; }
        public TimeSpan TimeSinceFetch { get; set; }
        public int? NumReports { get; set; }
        public int? ReportCount { get; set; }
        public string ApprovedBy { get; set; }
        public string AuthorFlairCssClass { get; set; }
        public string AuthorFlairText { get; set; }
        public string AuthorName { get; set; }
        public string BannedBy { get; set; }
        public int CommentCount { get; set; }
        public string Domain { get; set; }
        public int Downvotes { get; set; }
        public bool Edited { get; set; }
        public string FullName { get; set; }
        public int Gilded { get; set; }
        public string Id { get; set; }
        public bool IsArchived { get; set; }
        public bool IsSelfPost { get; set; }
        public bool IsSpoiler { get; set; }
        public bool IsStickied { get; set; }
        public string Kind { get; set; }
        public string LinkFlairCssClass { get; set; }
        public string LinkFlairText { get; set; }
        public bool NSFW { get; set; }
        public Uri Permalink { get; set; }
        public bool Saved { get; set; }
        public int Score { get; set; }
        public string SelfText { get; set; }
        public string SelfTextHtml { get; set; }
        public string Shortlink { get; set; }
        public string SubredditName { get; set; }
        public Uri Thumbnail { get; set; }
        public string Title { get; set; }
        public int Upvotes { get; set; }
        public Uri Url { get; set; }
    }
}