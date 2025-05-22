using System;
using System.Windows;
using System.Windows.Media;
using EMS.Models;

namespace EMS.Services
{
    public class ThemeManager
    {
        private static ThemeManager? _instance;
        private readonly ResourceDictionary _appResources;

        public static ThemeManager Instance
        {
            get
            {
                _instance ??= new ThemeManager();
                return _instance;
            }
        }

        private ThemeManager()
        {
            _appResources = Application.Current.Resources;
        }

        public void ApplyTheme(ThemePreference theme)
        {
            switch (theme)
            {
                case ThemePreference.Light:
                    ApplyLightTheme();
                    break;
                case ThemePreference.Dark:
                    ApplyDarkTheme();
                    break;
                case ThemePreference.System:
                    ApplySystemTheme();
                    break;
            }
        }

        private void ApplyLightTheme()
        {
            _appResources["BackgroundBrush"] = new SolidColorBrush((Color)_appResources["LightBackground"]);
            _appResources["ForegroundBrush"] = new SolidColorBrush((Color)_appResources["LightForeground"]);
            _appResources["AccentBrush"] = new SolidColorBrush((Color)_appResources["LightAccent"]);
            _appResources["BorderBrush"] = new SolidColorBrush((Color)_appResources["LightBorder"]);
        }

        private void ApplyDarkTheme()
        {
            _appResources["BackgroundBrush"] = new SolidColorBrush((Color)_appResources["DarkBackground"]);
            _appResources["ForegroundBrush"] = new SolidColorBrush((Color)_appResources["DarkForeground"]);
            _appResources["AccentBrush"] = new SolidColorBrush((Color)_appResources["DarkAccent"]);
            _appResources["BorderBrush"] = new SolidColorBrush((Color)_appResources["DarkBorder"]);
        }

        private void ApplySystemTheme()
        {
            // Get system theme
            var isDarkMode = SystemParameters.UxThemeName.Contains("Dark");
            if (isDarkMode)
            {
                ApplyDarkTheme();
            }
            else
            {
                ApplyLightTheme();
            }
        }
    }
} 