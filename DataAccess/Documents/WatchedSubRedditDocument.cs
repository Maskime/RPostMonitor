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

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccess.Documents
{
    public class WatchedSubRedditDocument:IWatchedSubReddit
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string SubRedditName { get; set; }

        public TimeSpan WatchedTime { get; set; }

        public DateTime PollerStartedAtUtc { get; set; }

        public DateTime LastTickAtUtc { get; set; }
    }
}
