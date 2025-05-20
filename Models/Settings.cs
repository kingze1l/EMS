using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EMS.Models
{
    public class Settings
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public required string UserId { get; set; }
        public required ThemePreference Theme { get; set; }
        public required LanguagePreference Language { get; set; }
        public bool IsAdminSettings { get; set; }
        public int DefaultPaginationSize { get; set; } = 10;
        public List<DefaultRole> DefaultRoles { get; set; } = new();
    }

    public enum ThemePreference
    {
        Light,
        Dark,
        System
    }

    public enum LanguagePreference
    {
        English,
        Spanish,
        French,
        German
    }

    public class DefaultRole
    {
        public int RoleId { get; set; }
        public required string RoleName { get; set; }
        public bool IsDefault { get; set; }
    }
} 