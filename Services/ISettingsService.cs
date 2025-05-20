using EMS.Models;
using System.Threading.Tasks;

namespace EMS.Services
{
    public interface ISettingsService
    {
        Task<Settings> GetUserSettingsAsync(string userId);
        Task<Settings> GetAdminSettingsAsync();
        Task<bool> UpdateUserSettingsAsync(Settings settings);
        Task<bool> UpdateAdminSettingsAsync(Settings settings);
        Task<bool> InitializeDefaultSettingsAsync(string userId);
    }
} 