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
            var backgroundBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            backgroundBrush.GradientStops.Add(new GradientStop((Color)_appResources["LightBackground"], 0));
            backgroundBrush.GradientStops.Add(new GradientStop((Color)_appResources["LightBackgroundSecondary"], 1));

            _appResources["BackgroundBrush"] = backgroundBrush;
            _appResources["ForegroundBrush"] = new SolidColorBrush((Color)_appResources["LightForeground"]);
            _appResources["AccentBrush"] = new SolidColorBrush((Color)_appResources["LightAccent"]);
            _appResources["BorderBrush"] = new SolidColorBrush((Color)_appResources["LightBorder"]);
            _appResources["AlternateRowBrush"] = new SolidColorBrush((Color)_appResources["LightAlternateRow"]);
            _appResources["HeaderBackgroundBrush"] = new SolidColorBrush((Color)_appResources["LightHeaderBackground"]);
        }

        private void ApplyDarkTheme()
        {
            var backgroundBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            backgroundBrush.GradientStops.Add(new GradientStop((Color)_appResources["DarkBackground"], 0));
            backgroundBrush.GradientStops.Add(new GradientStop((Color)_appResources["DarkBackgroundSecondary"], 1));

            _appResources["BackgroundBrush"] = backgroundBrush;
            _appResources["ForegroundBrush"] = new SolidColorBrush((Color)_appResources["DarkForeground"]);
            _appResources["AccentBrush"] = new SolidColorBrush((Color)_appResources["DarkAccent"]);
            _appResources["BorderBrush"] = new SolidColorBrush((Color)_appResources["DarkBorder"]);
            _appResources["AlternateRowBrush"] = new SolidColorBrush((Color)_appResources["DarkAlternateRow"]);
            _appResources["HeaderBackgroundBrush"] = new SolidColorBrush((Color)_appResources["DarkHeaderBackground"]);
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