using EMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMS.Services
{
    public interface IRoleService
    {
        Task<List<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(string id);
        Task<Role> CreateRoleAsync(Role role);
        Task UpdateRoleAsync(Role role);
        Task DeleteRoleAsync(string id);
        Task<List<string>> GetAllPermissionsAsync();
        Task<List<PermissionDefinition>> GetPermissionsByCategoryAsync(string category);
    }
} 