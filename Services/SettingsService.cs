using EMS.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EMS.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IMongoCollection<Settings> _settingsCollection;

        public SettingsService(IMongoDatabase database)
        {
            _settingsCollection = database.GetCollection<Settings>("Settings");
        }

        public async Task<Settings> GetUserSettingsAsync(string userId)
        {
            var settings = await _settingsCollection.Find(s => s.UserId == userId && !s.IsAdminSettings).FirstOrDefaultAsync();
            if (settings == null)
            {
                await InitializeDefaultSettingsAsync(userId);
                settings = await _settingsCollection.Find(s => s.UserId == userId && !s.IsAdminSettings).FirstOrDefaultAsync();
            }
            return settings;
        }

        public async Task<Settings> GetAdminSettingsAsync()
        {
            var settings = await _settingsCollection.Find(s => s.IsAdminSettings).FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new Settings
                {
                    UserId = "admin",
                    Theme = ThemePreference.System,
                    Language = LanguagePreference.English,
                    IsAdminSettings = true,
                    DefaultPaginationSize = 10,
                    DefaultRoles = new List<DefaultRole>
                    {
                        new DefaultRole { RoleId = 1, RoleName = "Admin", IsDefault = true },
                        new DefaultRole { RoleId = 2, RoleName = "Employee", IsDefault = true }
                    }
                };
                await _settingsCollection.InsertOneAsync(settings);
            }
            return settings;
        }

        public async Task<bool> UpdateUserSettingsAsync(Settings settings)
        {
            var result = await _settingsCollection.ReplaceOneAsync(
                s => s.UserId == settings.UserId && !s.IsAdminSettings,
                settings,
                new ReplaceOptions { IsUpsert = true });
            return result.IsAcknowledged;
        }

        public async Task<bool> UpdateAdminSettingsAsync(Settings settings)
        {
            var result = await _settingsCollection.ReplaceOneAsync(
                s => s.IsAdminSettings,
                settings,
                new ReplaceOptions { IsUpsert = true });
            return result.IsAcknowledged;
        }

        public async Task<bool> InitializeDefaultSettingsAsync(string userId)
        {
            var defaultSettings = new Settings
            {
                UserId = userId,
                Theme = ThemePreference.System,
                Language = LanguagePreference.English,
                IsAdminSettings = false,
                DefaultPaginationSize = 10
            };

            await _settingsCollection.InsertOneAsync(defaultSettings);
            return true;
        }
    }
} 