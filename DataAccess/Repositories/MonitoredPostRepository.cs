using System;
using System.Collections.Generic;

using AutoMapper;

using Common.Errors;
using Common.Model.Document;
using Common.Model.Repositories;
using Common.Reddit;

using DataAccess.Config;
using DataAccess.Documents;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MongoDB.Driver;

namespace DataAccess.Repositories
{
    public class MonitoredPostRepository : IMonitoredPostRepository
    {
        private readonly IMapper _mapper;
        private readonly ILogger<MonitoredPostRepository> _logger;

        private readonly IMongoCollection<RedditMonitoredPostDocument> _posts;
        private readonly IMongoCollection<RedditMonitoredPostVersionDocument> _postVersions;

        public MonitoredPostRepository(
            IOptions<DatabaseSettings> settings
            , IMapper mapper
            , ILogger<MonitoredPostRepository> logger
        )
        {
            DatabaseSettings settingsValue = settings.Value;
            _mapper = mapper;
            _logger = logger;
            var client = new MongoClient(settingsValue.ConnectionString);

            var database = client.GetDatabase(settingsValue.DatabaseName);

            _posts = database.GetCollection<RedditMonitoredPostDocument>(settingsValue.MonitoredPostsCollectionName);
            _postVersions =
                database.GetCollection<RedditMonitoredPostVersionDocument>(settingsValue.MonitoredPostVersionsCollectionName);
        }

        public bool Insert(IRedditPost redditPost)
        {
            long countDocuments = _posts.CountDocuments(p => p.FullName.Equals(redditPost.FullName));
            if (countDocuments > 0)
            {
                _logger.LogWarning("Post with FullName [{}] already watched, skipping", redditPost.FullName);
                return false;
            }

            var document = _mapper.Map<RedditMonitoredPostDocument>(redditPost);
            document.IterationNumber = 1;
            document.Age = DateTimeOffset.UtcNow - document.CreatedUTC;
            _posts.InsertOne(document);
            return true;
        }

        public List<IRedditMonitoredPost> FindPostToUpdate(int lastFetchOlderThanInSeconds,
            int maxNumberOfIterations,
            long configInactivityTimeoutInHours,
            int maxSimultaneousFetch, TimeSpan maxPostAgeInDays)
        {
            var dateTimeOffset = DateTimeOffset.UtcNow;
            var maxLastFetch = dateTimeOffset.AddSeconds(lastFetchOlderThanInSeconds * -1);

            var sort = Builders<RedditMonitoredPostDocument>.Sort
                                                            .Ascending(p => p.FetchedAt);
            TimeSpan maxInactivityAge = TimeSpan.FromHours(configInactivityTimeoutInHours);
            try
            {
                var toUpdate = new List<IRedditMonitoredPost>(_posts
                                                              .Find(post =>
                                                                  post.FetchedAt <= maxLastFetch
                                                                  //Marked as deleted
                                                                  && !post.SelfTextHtml.Contains("SC_OFF")
                                                                  // A flag to avoid to retrieve posts that are currently being fetched
                                                                  && !post.IsFetching
                                                                  // Max number of times to fetch the post.
                                                                  && post.Age < maxPostAgeInDays
                                                                  // Post that have been inactive for too long should not be fetched anymore.
                                                                  && post.InactivityAge < maxInactivityAge
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
            return _posts.CountDocuments(p => true);
        }

        public IRedditMonitoredPost Get(string fullName)
        {
            return _posts.Find(p => p.FullName == fullName).FirstOrDefault();
        }

        public void AddVersion(IRedditPost newVersion)
        {
            var newPostVersion = _mapper.Map<RedditMonitoredPostDocument>(newVersion);
            if (!(Get(newPostVersion.FullName) is RedditMonitoredPostDocument oldVersion))
            {
                throw new PostMonitorException($"Could not find original version for post id [{newVersion.Id}]");
            }

            newPostVersion.IterationNumber = oldVersion.IterationNumber + 1;
            newPostVersion.InactivityAge = TimeSpan.Zero;
            TimeSpan postAge = DateTimeOffset.UtcNow - oldVersion.CreatedUTC;
            newPostVersion.Age = postAge;

            newPostVersion.Id = oldVersion.Id;
            _posts.ReplaceOne(p => p.FullName == newPostVersion.FullName, newPostVersion);
            
            _postVersions.InsertOne(_mapper.Map<RedditMonitoredPostVersionDocument>(oldVersion));
        }

        public void SetFetching(string fullName, bool isFetching)
        {
            var update = Builders<RedditMonitoredPostDocument>.Update.Set(p => p.IsFetching, isFetching);
            _posts.UpdateOne(p => p.FullName == fullName, update);
        }

        public long CountPostWithMissingIterations(int configNbIterationOnPost)
        {
            return _posts.CountDocuments(p =>
                    p.IterationNumber < configNbIterationOnPost
                    && !p.SelfTextHtml.Contains("SC_OFF") //Marked as deleted
            );
        }

        public void UpdatePostInactivity(IRedditMonitoredPost lastVersion)
        {
            TimeSpan inactivityAge = lastVersion.InactivityAge + (DateTimeOffset.Now - lastVersion.FetchedAt);
            TimeSpan age = DateTimeOffset.UtcNow - lastVersion.CreatedUTC;
            UpdateDefinition<RedditMonitoredPostDocument> update = Builders<RedditMonitoredPostDocument>
                                                                   .Update
                                                                   .Set(p => p.InactivityAge, inactivityAge)
                                                                   .Set(p => p.FetchedAt, DateTimeOffset.Now)
                                                                   .Set(p => p.Age, age)
                ;
            _posts.UpdateOne(p => p.FullName == lastVersion.FullName, update);
        }

        public void SetFetchingAll(bool isFetching)
        {
            var update = Builders<RedditMonitoredPostDocument>.Update.Set(p => p.IsFetching, isFetching);
            _posts.UpdateMany(p => true, update);
        }

        public List<IRedditMonitoredPost> FindAllPostAndVersions()
        {
            var sort = Builders<RedditMonitoredPostDocument>
                       .Sort.Ascending(p => p.FullName);
            var allPosts = _posts.Find(p => true)
                                 .Sort(sort)
                                 .ToList();
            var versionSort = Builders<RedditMonitoredPostVersionDocument>
                              .Sort.Ascending(p => p.IterationNumber);
            var output = new List<IRedditMonitoredPost>();
            foreach (var post in allPosts)
            {
                output.Add(post);
                output.AddRange(_postVersions
                                .Find(p => p.FullName.Equals(post.FullName))
                                .Sort(versionSort)
                                .ToList());
            }

            return output;
        }
    }
}
