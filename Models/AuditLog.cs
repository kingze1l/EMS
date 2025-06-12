using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EMS.Models
{
    public class AuditLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;  // e.g., "Create", "Update", "Delete", "Login", "Logout"
        public string EntityType { get; set; } = string.Empty;  // e.g., "Employee", "Role", "Settings"
        public string EntityId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; } = string.Empty;
    }
} 