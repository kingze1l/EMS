using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using EMS.Services;
using EMS.Views;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace EMS
{
    public partial class MainWindow : Window
    {
        private readonly AuthenticationService _authService;
        private readonly IEmployeeService? _employeeService;
        private Frame? _mainFrame;

        public MainWindow(AuthenticationService authService)
        {
            InitializeComponent();
            _authService = authService;
            
            var app = Application.Current as App;
            if (app?.ServiceProvider != null)
            {
                _employeeService = app.ServiceProvider.GetService<IEmployeeService>();
            }
            
            _mainFrame = FindName("MainFrame") as Frame;
            
            SetupNotifications();
            UpdateUIForCurrentUser();
        }

        private void UpdateUIForCurrentUser()
        {
            if (_authService.CurrentUser != null)
            {
                // Find the WelcomeText TextBlock by name
                if (FindName("WelcomeText") is TextBlock welcomeText)
                {
                    welcomeText.Text = $"Welcome, {_authService.CurrentUser.Name}!";
                }

                // Find the AdminPanel by name
                if (FindName("AdminPanel") is StackPanel adminPanel)
                {
                    adminPanel.Visibility = _authService.IsUserInRole("Admin") ? 
                        Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void SetupNotifications()
        {
            // TODO: Setup real-time notifications
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            if (_mainFrame != null)
            {
                // TODO: Navigate to Dashboard view
                _mainFrame.Content = null;
            }
        }

        private void Attendance_Click(object sender, RoutedEventArgs e)
        {
            if (_mainFrame != null)
            {
                // TODO: Navigate to Attendance view
                _mainFrame.Content = null;
            }
        }

        private void Employees_Click(object sender, RoutedEventArgs e)
        {
            if (_mainFrame != null && _authService.CurrentUser != null && _employeeService != null)
            {
                _mainFrame.Content = new EmployeeView(_employeeService, _authService.CurrentUser.UserRole);
            }
        }

        private void Analytics_Click(object sender, RoutedEventArgs e)
        {
            if (_mainFrame != null)
            {
                // TODO: Navigate to Analytics view
                _mainFrame.Content = null;
            }
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            if (_mainFrame != null)
            {
                // TODO: Navigate to Report view
                _mainFrame.Content = null;
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (_mainFrame != null)
            {
                // TODO: Navigate to Settings view
                _mainFrame.Content = null;
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Logout", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _authService.Logout();
                var loginWindow = new LoginWindow(_authService);
                loginWindow.Show();
                this.Close();
            }
        }

        private void SendWishes_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Birthday wishes sent!", "Birthday Wishes", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox searchBox && !string.IsNullOrWhiteSpace(searchBox.Text))
            {
                // TODO: Implement search functionality
                // You can now use _authService.CurrentUser to get the current user's context
            }
        }

        private void Notification_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("No new notifications", "Notifications", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}