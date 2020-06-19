using System;

namespace Common.Reddit
{
    public interface IRedditPost
    {
        string Author { get; set; }
        DateTimeOffset CreatedAt { get; set; }
        DateTimeOffset FetchedAt { get; set; }
        string RedditId { get; set; }
        string Permalink { get; set; }
        string Url { get; set; }

        string Title { get; set; }
    }
}