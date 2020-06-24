using System;

namespace Common.Reddit
{
    public interface IRedditPost
    {
        DateTimeOffset Created { get; set; }
        DateTimeOffset CreatedUTC { get; set; }
        DateTimeOffset FetchedAt { get; set; }

        TimeSpan TimeSinceFetch { get; set; }

        Nullable<int> NumReports { get; set; }
        Nullable<int> ReportCount { get; set; }

        string ApprovedBy { get; set; }
        string AuthorFlairCssClass { get; set; }
        string AuthorFlairText { get; set; }
        string AuthorName { get; set; }
        string BannedBy { get; set; }
        int CommentCount { get; set; }
        string Domain { get; set; }
        int Downvotes { get; set; }
        bool Edited { get; set; }
        string FullName { get; set; }
        int Gilded { get; set; }
        string Id { get; set; }
        bool IsArchived { get; set; }
        bool IsSelfPost { get; set; }
        bool IsSpoiler { get; set; }
        bool IsStickied { get; set; }
        string Kind { get; set; }
        string LinkFlairCssClass { get; set; }
        string LinkFlairText { get; set; }
        bool NSFW { get; set; }
        Uri Permalink { get; set; }
        bool Saved { get; set; }
        int Score { get; set; }
        string SelfText { get; set; }
        string SelfTextHtml { get; set; }
        string Shortlink { get; set; }
        string SubredditName { get; set; }
        Uri Thumbnail { get; set; }
        string Title { get; set; }
        int Upvotes { get; set; }
        Uri Url { get; set; }
    }
}
