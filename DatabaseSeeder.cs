using MongoDB.Driver;
using EMS.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EMS
{
    public class DatabaseSeeder
    {
        private readonly IMongoCollection<Employee> _employeeCollection;
        private readonly IMongoCollection<UserRole> _roleCollection;

        public DatabaseSeeder(IMongoDatabase database)
        {
            _employeeCollection = database.GetCollection<Employee>("Employees");
            _roleCollection = database.GetCollection<UserRole>("Roles");
        }

        public async Task SeedDataAsync()
        {
            // Check if data already exists
            if (await _employeeCollection.CountDocumentsAsync(FilterDefinition<Employee>.Empty) > 0)
            {
                return;
            }

            // Create roles
            var adminRole = new UserRole 
            { 
                RoleID = 1, 
                RoleName = "Admin", 
                Permissions = new List<Permission> 
                { 
                    Permission.ViewEmployees, 
                    Permission.EditEmployees, 
                    Permission.ViewReports, 
                    Permission.EditRoles, 
                    Permission.ManageUsers 
                } 
            };

            var managerRole = new UserRole 
            { 
                RoleID = 2, 
                RoleName = "Manager", 
                Permissions = new List<Permission> 
                { 
                    Permission.ViewEmployees, 
                    Permission.EditEmployees, 
                    Permission.ViewReports 
                } 
            };

            var hrRole = new UserRole 
            { 
                RoleID = 3, 
                RoleName = "HR", 
                Permissions = new List<Permission> 
                { 
                    Permission.ViewEmployees, 
                    Permission.ManageUsers 
                } 
            };

            // Insert roles
            await _roleCollection.InsertManyAsync(new[] { adminRole, managerRole, hrRole });

            // Create initial employees
            var employees = new List<Employee>
            {
                new Employee
                {
                    Name = "Admin User",
                    Position = "Administrator",
                    Contact = "admin@company.com",
                    Username = "admin",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    UserRole = adminRole,
                    DateOfBirth = new DateTime(1980, 5, 15)
                },
                new Employee
                {
                    Name = "Manager User",
                    Position = "Manager",
                    Contact = "manager@company.com",
                    Username = "manager",
                    Password = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    UserRole = managerRole,
                    DateOfBirth = new DateTime(1985, 3, 10)
                },
                new Employee
                {
                    Name = "HR User",
                    Position = "HR",
                    Contact = "hr@company.com",
                    Username = "hr",
                    Password = BCrypt.Net.BCrypt.HashPassword("hr123"),
                    UserRole = hrRole,
                    DateOfBirth = new DateTime(1990, 8, 22)
                }
            };

            // Insert employees
            await _employeeCollection.InsertManyAsync(employees);
        }
    }
} 