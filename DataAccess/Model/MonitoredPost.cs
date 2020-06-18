using System;
using Common.Model.Document;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataAccess.Model
{
    public class MonitoredPost:IMonitoredPost
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Author { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime FetchedAt { get; set; }
        public string RedditId { get; set; }
        public string Permalink { get; set; }
        public string Url { get; set; }
    }
}