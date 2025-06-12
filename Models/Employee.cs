using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace EMS.Models
{
    public enum Permission
    {
        ViewEmployees,
        EditEmployees,
        ViewReports,
        EditRoles,
        ManageUsers,
        ViewPayroll,
        EditPayroll,
        GeneratePayroll,
        ViewPayrollHistory,
        // Add more as needed
    }

    public class UserRole
    {
        public int RoleID { get; set; }
        public required string RoleName { get; set; }
        public List<Permission> Permissions { get; set; } = new();
    }

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
        public decimal BasePay { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }
    }
}

// Example roles (to be seeded in DB):
// Admin: All permissions
// Manager: ViewEmployees, EditEmployees, ViewReports
// HR: ViewEmployees, ManageUsers 