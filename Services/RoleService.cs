using EMS.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace EMS.Services
{
    public class RoleService : IRoleService
    {
        private readonly IMongoCollection<Role> _roles;
        private readonly IMongoCollection<PermissionDefinition> _permissions;

        public RoleService(IMongoDatabase database)
        {
            _roles = database.GetCollection<Role>("roles");
            _permissions = database.GetCollection<PermissionDefinition>("permissions");
            InitializePermissions();
            InitializeSystemRoles();
        }

        private async void InitializePermissions()
        {
            var permissions = new List<PermissionDefinition>
            {
                // Leave Permissions
                new PermissionDefinition { Name = "CanViewLeaves", Description = "Can view leave requests", Category = PermissionDefinition.Categories.Leave },
                new PermissionDefinition { Name = "CanApproveLeaves", Description = "Can approve or reject leave requests", Category = PermissionDefinition.Categories.Leave },
                new PermissionDefinition { Name = "CanRequestLeaves", Description = "Can request leaves", Category = PermissionDefinition.Categories.Leave },

                // Payroll Permissions
                new PermissionDefinition { Name = "CanViewPayroll", Description = "Can view payroll information", Category = PermissionDefinition.Categories.Payroll },
                new PermissionDefinition { Name = "CanEditPayroll", Description = "Can edit payroll information", Category = PermissionDefinition.Categories.Payroll },

                // Employee Permissions
                new PermissionDefinition { Name = "CanViewEmployees", Description = "Can view employee information", Category = PermissionDefinition.Categories.Employee },
                new PermissionDefinition { Name = "CanEditEmployees", Description = "Can edit employee information", Category = PermissionDefinition.Categories.Employee },
                new PermissionDefinition { Name = "CanDeleteEmployees", Description = "Can delete employees", Category = PermissionDefinition.Categories.Employee },

                // Role Permissions
                new PermissionDefinition { Name = "CanViewRoles", Description = "Can view roles", Category = PermissionDefinition.Categories.Role },
                new PermissionDefinition { Name = "CanEditRoles", Description = "Can edit roles", Category = PermissionDefinition.Categories.Role },
                new PermissionDefinition { Name = "CanDeleteRoles", Description = "Can delete roles", Category = PermissionDefinition.Categories.Role },

                // Department Permissions
                new PermissionDefinition { Name = "CanViewDepartments", Description = "Can view departments", Category = PermissionDefinition.Categories.Department },
                new PermissionDefinition { Name = "CanEditDepartments", Description = "Can edit departments", Category = PermissionDefinition.Categories.Department },
                new PermissionDefinition { Name = "CanDeleteDepartments", Description = "Can delete departments", Category = PermissionDefinition.Categories.Department },

                // Dashboard Permissions
                new PermissionDefinition { Name = "CanViewDashboard", Description = "Can view dashboard", Category = PermissionDefinition.Categories.Dashboard },
                new PermissionDefinition { Name = "CanViewAnalytics", Description = "Can view analytics", Category = PermissionDefinition.Categories.Dashboard }
            };

            foreach (var permission in permissions)
            {
                var filter = Builders<PermissionDefinition>.Filter.Eq(p => p.Name, permission.Name);
                var update = Builders<PermissionDefinition>.Update.SetOnInsert(p => p.Name, permission.Name)
                                                      .SetOnInsert(p => p.Description, permission.Description)
                                                      .SetOnInsert(p => p.Category, permission.Category);
                await _permissions.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            }
        }

        private async void InitializeSystemRoles()
        {
            var roles = new List<Role>
            {
                new Role
                {
                    RoleName = "Administrator",
                    Type = RoleType.Admin,
                    Description = "Full system access",
                    IsSystemRole = true,
                    Permissions = new List<string> { "CanViewLeaves", "CanApproveLeaves", "CanRequestLeaves",
                        "CanViewPayroll", "CanEditPayroll", "CanViewEmployees", "CanEditEmployees", "CanDeleteEmployees",
                        "CanViewRoles", "CanEditRoles", "CanDeleteRoles", "CanViewDepartments", "CanEditDepartments",
                        "CanDeleteDepartments", "CanViewDashboard", "CanViewAnalytics" }
                },
                new Role
                {
                    RoleName = "Manager",
                    Type = RoleType.Manager,
                    Description = "Department management access",
                    IsSystemRole = true,
                    Permissions = new List<string> { "CanViewLeaves", "CanApproveLeaves", "CanRequestLeaves",
                        "CanViewEmployees", "CanViewDepartments", "CanViewDashboard" }
                },
                new Role
                {
                    RoleName = "HR",
                    Type = RoleType.HR,
                    Description = "Human Resources access",
                    IsSystemRole = true,
                    Permissions = new List<string> { "CanViewLeaves", "CanApproveLeaves", "CanRequestLeaves",
                        "CanViewPayroll", "CanEditPayroll", "CanViewEmployees", "CanEditEmployees",
                        "CanViewDepartments", "CanViewDashboard" }
                },
                new Role
                {
                    RoleName = "Employee",
                    Type = RoleType.Employee,
                    Description = "Basic employee access",
                    IsSystemRole = true,
                    Permissions = new List<string> { "CanRequestLeaves", "CanViewEmployees", "CanViewDashboard" }
                }
            };

            foreach (var role in roles)
            {
                var filter = Builders<Role>.Filter.Eq(r => r.RoleName, role.RoleName);
                var update = Builders<Role>.Update.SetOnInsert(r => r.RoleName, role.RoleName)
                                                .SetOnInsert(r => r.Type, role.Type)
                                                .SetOnInsert(r => r.Description, role.Description)
                                                .SetOnInsert(r => r.IsSystemRole, role.IsSystemRole)
                                                .SetOnInsert(r => r.Permissions, role.Permissions);
                await _roles.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            }
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _roles.Find(_ => true).ToListAsync();
        }

        public async Task<Role> GetRoleByIdAsync(string id)
        {
            return await _roles.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            role.IsSystemRole = false; // Ensure new roles are not system roles
            await _roles.InsertOneAsync(role);
            return role;
        }

        public async Task UpdateRoleAsync(Role role)
        {
            var existingRole = await GetRoleByIdAsync(role.Id);
            if (existingRole?.IsSystemRole == true)
            {
                throw new System.InvalidOperationException("Cannot modify system roles");
            }
            await _roles.ReplaceOneAsync(r => r.Id == role.Id, role);
        }

        public async Task DeleteRoleAsync(string id)
        {
            var role = await GetRoleByIdAsync(id);
            if (role?.IsSystemRole == true)
            {
                throw new System.InvalidOperationException("Cannot delete system roles");
            }
            await _roles.DeleteOneAsync(r => r.Id == id);
        }

        public async Task<List<string>> GetAllPermissionsAsync()
        {
            var permissions = await _permissions.Find(_ => true).ToListAsync();
            return permissions.Select(p => p.Name).ToList();
        }

        public async Task<List<PermissionDefinition>> GetPermissionsByCategoryAsync(string category)
        {
            return await _permissions.Find(p => p.Category == category).ToListAsync();
        }
    }
} 