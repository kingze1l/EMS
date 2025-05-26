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

            var adminRole = new UserRole { RoleID = 1, RoleName = "Admin", Permissions = new List<Permission> { Permission.ViewEmployees, Permission.EditEmployees, Permission.ViewReports, Permission.EditRoles, Permission.ManageUsers } };
            var managerRole = new UserRole { RoleID = 2, RoleName = "Manager", Permissions = new List<Permission> { Permission.ViewEmployees, Permission.EditEmployees, Permission.ViewReports } };
            var hrRole = new UserRole { RoleID = 3, RoleName = "HR", Permissions = new List<Permission> { Permission.ViewEmployees, Permission.ManageUsers } };

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
                    Name = "John Doe",
                    Position = "Developer",
                    Contact = "john@company.com",
                    Username = "john",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                    UserRole = managerRole,
                    DateOfBirth = new DateTime(1990, 8, 22)
                },
                new Employee
                {
                    Name = "Jane Smith",
                    Position = "HR",
                    Contact = "jane@company.com",
                    Username = "jane",
                    Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                    UserRole = hrRole,
                    DateOfBirth = new DateTime(1985, 3, 10)
                }
            };

            await _employeeCollection.InsertManyAsync(employees);

            // Example users
            var adminUser = new Employee { Name = "Admin User", Position = "Admin", Contact = "admin@ems.com", Username = "admin", Password = BCrypt.Net.BCrypt.HashPassword("admin123"), UserRole = adminRole, DateOfBirth = DateTime.Parse("1980-01-01") };
            var managerUser = new Employee { Name = "Manager User", Position = "Manager", Contact = "manager@ems.com", Username = "manager", Password = BCrypt.Net.BCrypt.HashPassword("manager123"), UserRole = managerRole, DateOfBirth = DateTime.Parse("1985-01-01") };
            var hrUser = new Employee { Name = "HR User", Position = "HR", Contact = "hr@ems.com", Username = "hr", Password = BCrypt.Net.BCrypt.HashPassword("hr123"), UserRole = hrRole, DateOfBirth = DateTime.Parse("1990-01-01") };

            // Insert roles and users if not present (pseudo-code, adapt to your seeding logic)
            // rolesCollection.InsertOne(adminRole); etc.
            // employeesCollection.InsertOne(adminUser); etc.
        }
    }
} 