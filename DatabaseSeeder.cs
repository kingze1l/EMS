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
        private readonly IMongoCollection<PermissionDefinition> _permissionCollection;

        public DatabaseSeeder(IMongoDatabase database)
        {
            _employeeCollection = database.GetCollection<Employee>("Employees");
            _roleCollection = database.GetCollection<Role>("roles");
            _permissionCollection = database.GetCollection<PermissionDefinition>("permissions");
        }

        public async Task SeedDataAsync()
        {
            // Check if data already exists
            if (await _employeeCollection.CountDocumentsAsync(FilterDefinition<Employee>.Empty) > 0)
            {
                // Update existing admin roles with payroll permissions
                await UpdateExistingAdminRolesAsync();
                // Ensure permission definitions exist
                await SeedPermissionDefinitionsAsync();
                return;
            }

            // Clear existing roles to ensure a fresh seed
            await _roleCollection.DeleteManyAsync(FilterDefinition<Role>.Empty);

            // Seed permission definitions first
            await SeedPermissionDefinitionsAsync();

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
                    Permission.ManageUsers.ToString(),
                    Permission.ViewPayroll.ToString(),
                    Permission.EditPayroll.ToString(),
                    Permission.GeneratePayroll.ToString(),
                    Permission.ViewPayrollHistory.ToString()
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
                    DateOfBirth = new DateTime(1980, 5, 15),
                    BasePay = 120000, // Realistic admin salary
                    Bonus = 10000,
                    Deductions = 2000
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
                    DateOfBirth = new DateTime(1985, 3, 10),
                    BasePay = 95000, // Realistic manager salary
                    Bonus = 7000,
                    Deductions = 1500
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
                    DateOfBirth = new DateTime(1990, 8, 22),
                    BasePay = 75000, // Realistic HR salary
                    Bonus = 4000,
                    Deductions = 1200
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
                    DateOfBirth = new DateTime(1995, 1, 1),
                    BasePay = 55000, // Realistic associate salary
                    Bonus = 2000,
                    Deductions = 800
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

        private async Task UpdateExistingAdminRolesAsync()
        {
            try
            {
                // Find the Admin role
                var adminRole = await _roleCollection.Find(r => r.RoleName == "Admin").FirstOrDefaultAsync();
                
                if (adminRole != null)
                {
                    // Check if payroll permissions are missing
                    var payrollPermissions = new List<string>
                    {
                        Permission.ViewPayroll.ToString(),
                        Permission.EditPayroll.ToString(),
                        Permission.GeneratePayroll.ToString(),
                        Permission.ViewPayrollHistory.ToString()
                    };

                    bool needsUpdate = false;
                    foreach (var permission in payrollPermissions)
                    {
                        if (!adminRole.Permissions.Contains(permission))
                        {
                            adminRole.Permissions.Add(permission);
                            needsUpdate = true;
                        }
                    }

                    // Update the role if needed
                    if (needsUpdate)
                    {
                        await _roleCollection.ReplaceOneAsync(r => r.Id == adminRole.Id, adminRole);
                        
                        // Update all employees with Admin role to have the new permissions
                        var adminEmployees = await _employeeCollection.Find(e => e.UserRole.RoleName == "Admin").ToListAsync();
                        
                        foreach (var employee in adminEmployees)
                        {
                            // Update employee's permissions to match the updated role
                            employee.UserRole.Permissions = ConvertPermissionStringsToEnums(adminRole.Permissions);
                            await _employeeCollection.ReplaceOneAsync(e => e.Id == employee.Id, employee);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw to prevent application startup issues
                System.Diagnostics.Debug.WriteLine($"Error updating admin roles: {ex.Message}");
            }
        }

        private async Task SeedPermissionDefinitionsAsync()
        {
            try
            {
                // Check if permissions already exist
                var existingPermissions = await _permissionCollection.Find(_ => true).ToListAsync();
                if (existingPermissions.Any())
                {
                    return; // Permissions already exist
                }

                var permissionDefinitions = new List<PermissionDefinition>
                {
                    // Employee permissions
                    new PermissionDefinition { Name = "ViewEmployees", Description = "View employee information", Category = PermissionDefinition.Categories.Employee },
                    new PermissionDefinition { Name = "EditEmployees", Description = "Edit employee information", Category = PermissionDefinition.Categories.Employee },
                    
                    // Payroll permissions
                    new PermissionDefinition { Name = "ViewPayroll", Description = "View payroll information", Category = PermissionDefinition.Categories.Payroll },
                    new PermissionDefinition { Name = "EditPayroll", Description = "Edit payroll information", Category = PermissionDefinition.Categories.Payroll },
                    new PermissionDefinition { Name = "GeneratePayroll", Description = "Generate payroll records", Category = PermissionDefinition.Categories.Payroll },
                    new PermissionDefinition { Name = "ViewPayrollHistory", Description = "View payroll history", Category = PermissionDefinition.Categories.Payroll },
                    
                    // Role permissions
                    new PermissionDefinition { Name = "EditRoles", Description = "Manage roles and permissions", Category = PermissionDefinition.Categories.Role },
                    
                    // User management permissions
                    new PermissionDefinition { Name = "ManageUsers", Description = "Manage user accounts", Category = PermissionDefinition.Categories.Employee },
                    
                    // Report permissions
                    new PermissionDefinition { Name = "ViewReports", Description = "View system reports", Category = PermissionDefinition.Categories.Dashboard }
                };

                await _permissionCollection.InsertManyAsync(permissionDefinitions);
            }
            catch (Exception ex)
            {
                // Log the error but don't throw to prevent application startup issues
                System.Diagnostics.Debug.WriteLine($"Error seeding permission definitions: {ex.Message}");
            }
        }
    }
} 