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

        public DatabaseSeeder(IMongoDatabase database)
        {
            _employeeCollection = database.GetCollection<Employee>("Employees");
        }

        public async Task SeedDataAsync()
        {
            if (await _employeeCollection.CountDocumentsAsync(FilterDefinition<Employee>.Empty) > 0)
            {
                // Data already exists
                return;
            }

            var employees = new List<Employee>
            {
                new Employee
                {
                    Name = "Admin User",
                    Position = "Administrator",
                    Contact = "admin@company.com",
                    Username = "admin",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"), // Hash the password
                    UserRole = new UserRole
                    {
                        RoleID = 1,
                        RoleName = "Admin"
                    },
                    DateOfBirth = new DateTime(1980, 5, 15)
                },
                new Employee
                {
                    Name = "John Doe",
                    Position = "Developer",
                    Contact = "john@company.com",
                    Username = "john",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                    UserRole = new UserRole
                    {
                        RoleID = 2,
                        RoleName = "Employee"
                    },
                    DateOfBirth = new DateTime(1990, 8, 22)
                },
                new Employee
                {
                    Name = "Jane Smith",
                    Position = "Manager",
                    Contact = "jane@company.com",
                    Username = "jane",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                    UserRole = new UserRole
                    {
                        RoleID = 3,
                        RoleName = "Admin"
                    },
                    DateOfBirth = new DateTime(1985, 3, 10)
                }
            };

            await _employeeCollection.InsertManyAsync(employees);
        }
    }
} 