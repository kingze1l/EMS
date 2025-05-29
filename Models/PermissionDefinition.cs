using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EMS.Models
{
    public class PermissionDefinition
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("category")]
        public string Category { get; set; } = string.Empty;

        public static class Categories
        {
            public const string Leave = "Leave";
            public const string Payroll = "Payroll";
            public const string Employee = "Employee";
            public const string Role = "Role";
            public const string Department = "Department";
            public const string Dashboard = "Dashboard";
        }
    }
} 