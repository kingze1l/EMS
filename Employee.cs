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
        public string Id { get; set; }

        public string Name { get; set; }
        public string Position { get; set; }
        public string Contact { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public UserRole UserRole { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    public class UserRole
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
    }
}