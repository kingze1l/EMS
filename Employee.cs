//Employee.cs


using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EMS.Models
{
    public class Employee
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public required string Name { get; set; }
        public required string Position { get; set; }
        public required string Contact { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required UserRole UserRole { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    public class UserRole
    {
        public int RoleID { get; set; }
        public required string RoleName { get; set; }
    }
}