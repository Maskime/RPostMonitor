using System;

namespace Common.Model.Document
{
    public interface IMonitoredPost
    {
        string Id { get; set; }
        string Author { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime FetchedAt { get; set; }
        string RedditId { get; set; }
        string Permalink { get; set; }
        string Url { get; set; }
    }
}