//notifacaition.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EMS.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}