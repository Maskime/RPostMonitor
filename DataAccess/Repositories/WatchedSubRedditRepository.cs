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
using System.Collections.Generic;
using System.Threading.Tasks;

using Common.Errors;
using Common.Model.Document;
using Common.Model.Repositories;

using DataAccess.Documents;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

namespace DataAccess.Repositories
{
    public class WatchedSubRedditRepository : IWatchedSubRedditRepository
    {
        private IDatabaseContext _context;
        private ILogger<WatchedSubRedditRepository> _logger;

        public WatchedSubRedditRepository(
            IDatabaseContext context
            , ILogger<WatchedSubRedditRepository> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        public void UpdatedPollerStartedAt(string watchedSubReddit)
        {
            WatchedSubRedditDocument document = GetWatchedSubRedditDocument(watchedSubReddit);
            if (document == null)
            {
                _logger.LogDebug("Creating WatchedSubRedditDocument for [{SubReddit}]", watchedSubReddit);
                document = new WatchedSubRedditDocument
                {
                    PollerStartedAtUtc = DateTime.UtcNow,
                    SubRedditName = watchedSubReddit,
                    WatchedTime = TimeSpan.Zero,
                    LastTickAtUtc = DateTime.UtcNow
                };
                _context.WatchedSubReddits.InsertOne(document);
            }
            else
            {
                var update = Builders<WatchedSubRedditDocument>
                             .Update
                             .Set(s => s.PollerStartedAtUtc, DateTime.UtcNow)
                             .Set(s => s.LastTickAtUtc, DateTime.UtcNow)
                    ;
                _context.WatchedSubReddits.UpdateOne(s => s.SubRedditName.Equals(watchedSubReddit), update);
            }
        }

        private WatchedSubRedditDocument GetWatchedSubRedditDocument(string watchedSubReddit)
        {
            WatchedSubRedditDocument document = _context
                                                .WatchedSubReddits
                                                .Find(s => s.SubRedditName.Equals(watchedSubReddit))
                                                .FirstOrDefault();
            return document;
        }

        public TimeSpan UpdateSubRedditWatchedTime(string watchedSubReddit)
        {
            WatchedSubRedditDocument document = GetWatchedSubRedditDocument(watchedSubReddit);
            if (document == null)
            {
                throw new PostMonitorException(
                    $"Inconsistent Database state, trying to update watch time but WatchedSubRedditDocument [{watchedSubReddit}] does not exits");
            }

            DateTime now = DateTime.UtcNow;
            TimeSpan watchedTime = document.WatchedTime + (now - document.LastTickAtUtc);
            _logger.LogDebug("Watching [{SubReddit}] for [{WatchedTime}]", watchedSubReddit, watchedTime);
            var update = Builders<WatchedSubRedditDocument>
                         .Update
                         .Set(s => s.WatchedTime, watchedTime)
                         .Set(s => s.LastTickAtUtc, now)
                ;
            _context.WatchedSubReddits.UpdateOne(s => s.SubRedditName.Equals(watchedSubReddit), update);
            return watchedTime;
        }

        public TimeSpan GetSubRedditWatchedTime(string watchedSubReddit)
        {
            WatchedSubRedditDocument document = GetWatchedSubRedditDocument(watchedSubReddit);
            if (document == null)
            {
                return TimeSpan.Zero;
            }

            return document.WatchedTime;
        }

        public async Task<List<IWatchedSubReddit>> FindAllAsync()
        {
            IAsyncCursor<WatchedSubRedditDocument> results = await _context
                                                                   .WatchedSubReddits
                                                                   .FindAsync(s => true);
            
        }
    }
}
