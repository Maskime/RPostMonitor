using System;
using System.Collections.Generic;

using AutoMapper;

using Common.Errors;
using Common.Model.Document;
using Common.Model.Repositories;
using Common.Reddit;

using DataAccess.Documents;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

namespace DataAccess.Repositories
{
    public class MonitoredPostRepository : IMonitoredPostRepository
    {
        private readonly IMapper _mapper;
        private readonly ILogger<MonitoredPostRepository> _logger;

        private readonly IDatabaseContext _context;

        public MonitoredPostRepository(
            IDatabaseContext context
            , IMapper mapper
            , ILogger<MonitoredPostRepository> logger
        )
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public bool Insert(IRedditPost redditPost)
        {
            long countDocuments = _context.MonitoredPosts.CountDocuments(p => p.FullName.Equals(redditPost.FullName));
            if (countDocuments > 0)
            {
                _logger.LogWarning("Post with FullName [{}] already watched, skipping", redditPost.FullName);
                return false;
            }

            var document = _mapper.Map<RedditMonitoredPostDocument>(redditPost);
            document.IterationNumber = 1;
            document.Age = DateTimeOffset.UtcNow - document.CreatedUTC;
            _context.MonitoredPosts.InsertOne(document);
            return true;
        }

        public List<IRedditMonitoredPost> FindPostToUpdate(
            int lastFetchOlderThanInSeconds,
            int maxSimultaneousFetch, 
            TimeSpan maxPostAgeInDays)
        {
            var dateTimeOffset = DateTimeOffset.UtcNow;
            var maxLastFetch = dateTimeOffset.AddSeconds(lastFetchOlderThanInSeconds * -1);

            var sort = Builders<RedditMonitoredPostDocument>.Sort
                                                            .Ascending(p => p.FetchedAt);
            try
            {
                var toUpdate = new List<IRedditMonitoredPost>(_context.MonitoredPosts
                                                                      .Find(post =>
                                                                          post.FetchedAt <= maxLastFetch
                                                                          // A flag to avoid to retrieve posts that are currently being fetched
                                                                          && !post.IsFetching
                                                                          // Max number of times to fetch the post.
                                                                          && post.Age < maxPostAgeInDays
                                                                      )
                                                                      .Sort(sort)
                                                                      .Limit(maxSimultaneousFetch)
                                                                      .ToList())
                    ;
                return toUpdate;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, @"Error when fetching post to update in DB");
                throw;
            }
        }

        public long CountMonitoredPosts()
        {
            return _context.MonitoredPosts.CountDocuments(p => true);
        }

        public IRedditMonitoredPost Get(string fullName)
        {
            return _context.MonitoredPosts.Find(p => p.FullName == fullName).FirstOrDefault();
        }

        public void AddVersion(IRedditPost newVersion)
        {
            var newPostVersion = _mapper.Map<RedditMonitoredPostDocument>(newVersion);
            if (!(Get(newPostVersion.FullName) is RedditMonitoredPostDocument oldVersion))
            {
                throw new PostMonitorException($"Could not find original version for post id [{newVersion.Id}]");
            }

            newPostVersion.IterationNumber = oldVersion.IterationNumber + 1;
            newPostVersion.Age = DateTimeOffset.UtcNow - oldVersion.CreatedUTC;

            newPostVersion.Id = oldVersion.Id;
            _context.MonitoredPosts.ReplaceOne(p => p.FullName == newPostVersion.FullName, newPostVersion);
            
            _context.MonitoredPostVersions.InsertOne(_mapper.Map<RedditMonitoredPostVersionDocument>(oldVersion));
        }

        public void SetFetching(string fullName, bool isFetching)
        {
            var update = Builders<RedditMonitoredPostDocument>.Update.Set(p => p.IsFetching, isFetching);
            _context.MonitoredPosts.UpdateOne(p => p.FullName == fullName, update);
        }

        public void SetFetchingAll(bool isFetching)
        {
            var update = Builders<RedditMonitoredPostDocument>.Update.Set(p => p.IsFetching, isFetching);
            _context.MonitoredPosts.UpdateMany(p => true, update);
        }

        public List<IRedditMonitoredPost> FindAllPostAndVersions()
        {
            var sort = Builders<RedditMonitoredPostDocument>
                       .Sort.Ascending(p => p.FullName);
            var allPosts = _context.MonitoredPosts.Find(p => true)
                                   .Sort(sort)
                                   .ToList();
            var versionSort = Builders<RedditMonitoredPostVersionDocument>
                              .Sort.Ascending(p => p.IterationNumber);
            var output = new List<IRedditMonitoredPost>();
            foreach (var post in allPosts)
            {
                output.Add(post);
                output.AddRange(_context.MonitoredPostVersions
                                        .Find(p => p.FullName.Equals(post.FullName))
                                        .Sort(versionSort)
                                        .ToList());
            }

            return output;
        }
    }
}
