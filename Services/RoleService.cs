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
            // Permissions and roles are now seeded by DatabaseSeeder
            // InitializePermissions();
            // InitializeSystemRoles();
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