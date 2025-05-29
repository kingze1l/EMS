using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace EMS.Models
{
    public class Role
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("roleName")]
        public string RoleName { get; set; } = string.Empty;

        [BsonElement("type")]
        [BsonRepresentation(BsonType.String)]
        public RoleType Type { get; set; }

        [BsonElement("permissions")]
        public List<string> Permissions { get; set; } = new List<string>();

        [BsonElement("isSystemRole")]
        public bool IsSystemRole { get; set; }

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;
    }
} 