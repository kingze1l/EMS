using MongoDB.Driver;
using EMS.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace EMS
{
    public class DatabaseSeeder
    {
        private readonly IMongoCollection<Employee> _employeeCollection;
        private readonly IMongoCollection<Role> _roleCollection;

        public DatabaseSeeder(IMongoDatabase database)
        {
            _employeeCollection = database.GetCollection<Employee>("Employees");
            _roleCollection = database.GetCollection<Role>("roles");
        }

        public async Task SeedDataAsync()
        {
            // Check if data already exists
            if (await _employeeCollection.CountDocumentsAsync(FilterDefinition<Employee>.Empty) > 0)
            {
                return;
            }

            // Create roles
            var adminRole = new Role 
            { 
                RoleName = "Admin", 
                Type = RoleType.Admin,
                Description = "Administrator role with full permissions",
                IsSystemRole = true,
                Permissions = new List<string> 
                { 
                    Permission.ViewEmployees.ToString(),
                    Permission.EditEmployees.ToString(),
                    Permission.ViewReports.ToString(),
                    Permission.EditRoles.ToString(),
                    Permission.ManageUsers.ToString()
                } 
            };

            var managerRole = new Role 
            { 
                RoleName = "Manager", 
                Type = RoleType.Manager,
                Description = "Manager role with reporting and employee view/edit permissions",
                IsSystemRole = true,
                Permissions = new List<string> 
                { 
                    Permission.ViewEmployees.ToString(),
                    Permission.EditEmployees.ToString(),
                    Permission.ViewReports.ToString()
                } 
            };

            var hrRole = new Role 
            { 
                RoleName = "HR", 
                Type = RoleType.HR,
                Description = "HR role with employee and user management permissions",
                IsSystemRole = true,
                Permissions = new List<string> 
                { 
                    Permission.ViewEmployees.ToString(),
                    Permission.ManageUsers.ToString()
                } 
            };

            var employeeRole = new Role 
            { 
                RoleName = "Employee", 
                Type = RoleType.Employee,
                Description = "Standard employee role with basic permissions",
                IsSystemRole = true,
                Permissions = new List<string> 
                { 
                    Permission.ViewEmployees.ToString()
                } 
            };

            // Insert roles
            await _roleCollection.InsertManyAsync(new[] { adminRole, managerRole, hrRole, employeeRole });

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
                    UserRole = new UserRole
                    {
                        RoleID = 1,
                        RoleName = adminRole.RoleName,
                        Permissions = ConvertPermissionStringsToEnums(adminRole.Permissions)
                    },
                    DateOfBirth = new DateTime(1980, 5, 15)
                },
                new Employee
                {
                    Name = "Manager User",
                    Position = "Manager",
                    Contact = "manager@company.com",
                    Username = "manager",
                    Password = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    UserRole = new UserRole
                    {
                        RoleID = 2,
                        RoleName = managerRole.RoleName,
                        Permissions = ConvertPermissionStringsToEnums(managerRole.Permissions)
                    },
                    DateOfBirth = new DateTime(1985, 3, 10)
                },
                new Employee
                {
                    Name = "HR User",
                    Position = "HR",
                    Contact = "hr@company.com",
                    Username = "hr",
                    Password = BCrypt.Net.BCrypt.HashPassword("hr123"),
                    UserRole = new UserRole
                    {
                        RoleID = 3,
                        RoleName = hrRole.RoleName,
                        Permissions = ConvertPermissionStringsToEnums(hrRole.Permissions)
                    },
                    DateOfBirth = new DateTime(1990, 8, 22)
                },
                new Employee
                {
                    Name = "Regular Employee",
                    Position = "Associate",
                    Contact = "employee@company.com",
                    Username = "employee",
                    Password = BCrypt.Net.BCrypt.HashPassword("employee123"),
                    UserRole = new UserRole
                    {
                        RoleID = 4,
                        RoleName = employeeRole.RoleName,
                        Permissions = ConvertPermissionStringsToEnums(employeeRole.Permissions)
                    },
                    DateOfBirth = new DateTime(1995, 1, 1)
                }
            };

            // Insert employees
            await _employeeCollection.InsertManyAsync(employees);
        }

        private List<Permission> ConvertPermissionStringsToEnums(List<string> permissionNames)
        {
            var permissions = new List<Permission>();
            if (permissionNames == null) return permissions;

            foreach (var name in permissionNames)
            {
                if (Enum.TryParse(name, out Permission permissionEnum))
                {
                    permissions.Add(permissionEnum);
                }
            }
            return permissions;
        }
    }
} 