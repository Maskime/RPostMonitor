using System;
using Common.Reddit;
using RedditSharp.Things;

namespace DataAccess.Reddit
{
    public class RedditPost:IRedditPost
    {
        public string Author { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset FetchedAt { get; set; }
        public string RedditId { get; set; }
        public string Permalink { get; set; }
        public string Url { get; set; }

        public string Title { get; set; }

        public static RedditPost From(Post post)
        {
            return new RedditPost
            {
                Author = post.AuthorName,
                CreatedAt = post.Created,
                FetchedAt = post.FetchedAt,
                Permalink = post.Permalink.ToString(),
                RedditId = post.Id,
                Url = post.Url.ToString(),
                Title = post.Title
            };
        }
    }
}