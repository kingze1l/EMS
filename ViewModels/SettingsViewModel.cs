using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using EMS.Models;
using EMS.Services;
using System.Windows;

namespace EMS.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly ISettingsService _settingsService;
        private readonly string _userId;
        private readonly bool _isAdmin;
        private Settings? _userSettings;
        private Settings? _adminSettings;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public SettingsViewModel(ISettingsService settingsService, string userId, bool isAdmin)
        {
            _settingsService = settingsService;
            _userId = userId;
            _isAdmin = isAdmin;

            SaveSettingsCommand = new RelayCommand(async () => await SaveSettings());

            // Initialize available options
            AvailableThemes = Enum.GetValues(typeof(ThemePreference)).Cast<ThemePreference>().ToList();
            AvailableLanguages = Enum.GetValues(typeof(LanguagePreference)).Cast<LanguagePreference>().ToList();

            // Load settings
            _ = LoadSettings();
        }

        public List<ThemePreference> AvailableThemes { get; }
        public List<LanguagePreference> AvailableLanguages { get; }

        public ThemePreference SelectedTheme
        {
            get => _userSettings?.Theme ?? ThemePreference.System;
            set
            {
                if (_userSettings != null)
                {
                    _userSettings.Theme = value;
                    OnPropertyChanged();
                }
            }
        }

        public LanguagePreference SelectedLanguage
        {
            get => _userSettings?.Language ?? LanguagePreference.English;
            set
            {
                if (_userSettings != null)
                {
                    _userSettings.Language = value;
                    OnPropertyChanged();
                }
            }
        }

        public int PaginationSize
        {
            get => _adminSettings?.DefaultPaginationSize ?? 10;
            set
            {
                if (_adminSettings != null)
                {
                    _adminSettings.DefaultPaginationSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<DefaultRole> DefaultRoles
        {
            get => _adminSettings?.DefaultRoles ?? new List<DefaultRole>();
            set
            {
                if (_adminSettings != null)
                {
                    _adminSettings.DefaultRoles = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsAdmin => _isAdmin;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveSettingsCommand { get; }

        private async Task LoadSettings()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                _userSettings = await _settingsService.GetUserSettingsAsync(_userId);
                if (_isAdmin)
                {
                    _adminSettings = await _settingsService.GetAdminSettingsAsync();
                }

                OnPropertyChanged(nameof(SelectedTheme));
                OnPropertyChanged(nameof(SelectedLanguage));
                OnPropertyChanged(nameof(PaginationSize));
                OnPropertyChanged(nameof(DefaultRoles));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading settings: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveSettings()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                if (_userSettings != null && await _settingsService.UpdateUserSettingsAsync(_userSettings))
                {
                    if (_isAdmin && _adminSettings != null)
                    {
                        await _settingsService.UpdateAdminSettingsAsync(_adminSettings);
                    }
                    MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ErrorMessage = "Failed to save settings";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving settings: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 