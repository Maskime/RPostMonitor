// ************************************************************************************************
// 
//  © 2019       General Electric Company
// 
//  Description  See class summary below.
// 
//  History      See source code control system.
// 
// ************************************************************************************************

using System;

using Common.Model.Document;

using CsvHelper.Configuration.Attributes;

namespace PostMonitor.Model
{
    public class CsvRow:IRedditMonitoredPost
    {
        [Index(0)]
        public string FullName { get; set; }
        [Index(1)]
        public string SubredditName { get; set; }
        [Index(2)]
        public int Score { get; set; }
        [Index(3)]
        public string Title { get; set; }
        [Index(4)]
        public int? NumReports { get; set; }
        [Index(5)]
        public int? ReportCount { get; set; }
        [Index(6)]
        public int CommentCount { get; set; }
        [Index(7)]
        public int Downvotes { get; set; }
        [Index(8)]
        public int Gilded { get; set; }
        [Index(9)]
        public int Upvotes { get; set; }
        [Index(10)]
        public int IterationNumber { get; set; }
        [Index(11)]
        public bool Edited { get; set; }
        [Index(12)]
        public bool IsArchived { get; set; }
        [Index(13)]
        public bool IsSelfPost { get; set; }
        [Index(14)]
        public bool IsSpoiler { get; set; }
        [Index(15)]
        public bool IsStickied { get; set; }
        [Index(16)]
        public bool NSFW { get; set; }
        [Index(16)]
        public bool Saved { get; set; }
        [Index(18)]
        public string Kind { get; set; }
        [Index(19)]
        public string LinkFlairCssClass { get; set; }
        [Index(20)]
        public string LinkFlairText { get; set; }
        [Index(21)]
        public string ApprovedBy { get; set; }
        [Index(22)]
        public string AuthorFlairCssClass { get; set; }
        [Index(23)]
        public string AuthorFlairText { get; set; }
        [Index(24)]
        public string AuthorName { get; set; }
        [Index(25)]
        public string BannedBy { get; set; }
        [Index(26)]
        public string Domain { get; set; }
        [Index(27)]
        public string SelfText { get; set; }
        [Index(28)]
        public string SelfTextHtml { get; set; }
        [Index(29)]
        public string Shortlink { get; set; }
        [Index(30)]
        public Uri Permalink { get; set; }
        [Index(31)]
        public Uri Thumbnail { get; set; }
        [Index(32)]
        public Uri Url { get; set; }
        [Index(32)]
        public TimeSpan InactivityAge { get; set; }
        [Index(34)]
        public DateTimeOffset Created { get; set; }
        [Index(35)]
        public DateTimeOffset CreatedUTC { get; set; }
        /* Ignored fields */
        [Ignore]
        public TimeSpan Age { get; set; }
        [Ignore]
        public DateTimeOffset FetchedAt { get; set; }
        [Ignore]
        public TimeSpan TimeSinceFetch { get; set; }
        [Ignore]
        public string Id { get; set; }
    }
}
