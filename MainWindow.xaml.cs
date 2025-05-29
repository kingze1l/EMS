using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using EMS.Services;
using EMS.Views;
using System;
using Microsoft.Extensions.DependencyInjection;
using EMS.Models; // Add this namespace to access the Permission enum
using EMS.ViewModels;  // Add this line

namespace EMS
{
    public partial class MainWindow : Window
    {
        private readonly AuthenticationService? _authService;
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
            if (_authService?.CurrentUser != null)
            {
                // Find the WelcomeText TextBlock by name
                if (FindName("WelcomeText") is TextBlock welcomeText)
                {
                    welcomeText.Text = $"Welcome, {_authService.CurrentUser.Name}!";
                }

                // Find the AdminPanel by name and control visibility based on permission
                if (FindName("AdminPanel") is StackPanel adminPanel)
                {
                    adminPanel.Visibility = _authService.HasPermission(EMS.Models.Permission.EditRoles) ? 
                        Visibility.Visible : Visibility.Collapsed;
                }
                
                // You can add similar logic for other UI elements based on permissions
                // For example, a menu item for Payroll might be hidden if the user doesn't have payroll permissions.
            }
            else
            {
                // If _authService or CurrentUser is null, hide admin panel
                if (FindName("AdminPanel") is StackPanel adminPanel)
                {
                    adminPanel.Visibility = Visibility.Collapsed;
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
                // Assuming Dashboard is accessible to all logged-in users
                // TODO: Navigate to Dashboard view
                _mainFrame.Content = null; 
            }
        }

        private void Attendance_Click(object sender, RoutedEventArgs e)
        {
            // Check for Attendance permission
            if (_authService != null && _authService.HasPermission(EMS.Models.Permission.ViewReports))
            {
                 if (_mainFrame != null)
                {
                    // TODO: Navigate to Attendance view
                    _mainFrame.Content = null;
                }
            }
            else if (_authService != null)
            {
                MessageBox.Show("You do not have permission to view Attendance.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Employees_Click(object sender, RoutedEventArgs e)
        {
            // Check for ViewEmployees permission
            if (_authService != null && _authService.HasPermission(EMS.Models.Permission.ViewEmployees))
            {
                if (_mainFrame != null && _authService.CurrentUser != null && _employeeService != null)
                {
                    _mainFrame.Content = new EmployeeView(_employeeService, _authService.CurrentUser.UserRole);
                }
            }
            else if (_authService != null)
            {
                MessageBox.Show("You do not have permission to view Employees.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Analytics_Click(object sender, RoutedEventArgs e)
        {
            // Check for ViewReports permission
            if (_authService != null && _authService.HasPermission(EMS.Models.Permission.ViewReports))
            {
                if (_mainFrame != null)
                {
                    // TODO: Navigate to Analytics view
                    _mainFrame.Content = null;
                }
            }
            else if (_authService != null)
            {
                 MessageBox.Show("You do not have permission to view Analytics.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            // Check for ViewReports permission
            if (_authService != null && _authService.HasPermission(EMS.Models.Permission.ViewReports))
            {
                if (_mainFrame != null)
                {
                    // TODO: Navigate to Report view
                    _mainFrame.Content = null;
                }
            }
            else if (_authService != null)
            {
                 MessageBox.Show("You do not have permission to view Reports.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // Check for ManageUsers permission (assuming settings includes user management or requires admin-level access)
            if (_authService != null && _authService.HasPermission(EMS.Models.Permission.ManageUsers))
            {
                if (_mainFrame != null)
                {
                    var app = Application.Current as App;
                    var settingsService = app?.ServiceProvider.GetService<EMS.Services.ISettingsService>();
                    var authService = app?.ServiceProvider.GetService<AuthenticationService>();
                    
                    // Add null check for local authService before accessing CurrentUser and IsUserInRole
                    var user = authService?.CurrentUser; 

                    if (settingsService != null && user != null && authService != null)
                    {
                        try
                        {
                            var viewModel = new EMS.ViewModels.SettingsViewModel(
                                settingsService,
                                user.Id,
                                authService.IsUserInRole("Admin")
                            );
                            var settingsView = new EMS.Views.SettingsView
                            {
                                DataContext = viewModel
                            };
                            _mainFrame.Content = settingsView;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error creating Settings view: {ex.Message}", "Error");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Required services or user information is missing.", "Error");
                    }
                }
            }
            else if (_authService != null)
            {
                MessageBox.Show("You do not have permission to view Settings.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Logout", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _authService?.Logout();
                // Assuming LoginWindow is the entry point after logout
                var loginWindow = new LoginWindow(_authService!); 
                loginWindow.Show();
                this.Close();
            }
        }

        private void SendWishes_Click(object sender, RoutedEventArgs e)
        {
            // Check for a relevant permission, e.g., ViewEmployees to see employee birthdays
            if (_authService != null && _authService.HasPermission(EMS.Models.Permission.ViewEmployees))
            {
                 MessageBox.Show("Birthday wishes functionality to be implemented.", "Birthday Wishes", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (_authService != null)
            {
                MessageBox.Show("You do not have permission to send birthday wishes.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox searchBox && !string.IsNullOrWhiteSpace(searchBox.Text))
            {
                // TODO: Implement search functionality
                // Accessing current user for context is already possible via _authService?.CurrentUser
            }
        }

        private void Notification_Click(object sender, RoutedEventArgs e)
        {
             // Check for a relevant permission, e.g., a specific Notification permission if exists, or generally accessible.
             // For now, assuming accessible to all logged-in users.
              if (_authService != null) // Add null check if needed, depending on if this requires authentication
             {
                 MessageBox.Show("No new notifications", "Notifications", 
                     MessageBoxButton.OK, MessageBoxImage.Information);
             }
        }

        private void RoleManagement_Click(object sender, RoutedEventArgs e)
        {
            // Check for EditRoles permission
            if (_authService != null && _authService.HasPermission(EMS.Models.Permission.EditRoles))
            {
                try
                {
                    if (_mainFrame != null)
                    {
                        var app = Application.Current as App;
                        var roleService = app?.ServiceProvider.GetService<IRoleService>();
                        
                        if (roleService != null)
                        {
                            var viewModel = new RoleManagementViewModel(roleService);
                            var view = new RoleManagementView
                            {
                                DataContext = viewModel
                            };
                            _mainFrame.Content = view;
                        }
                        else
                        {
                            MessageBox.Show("Role service is not available.", "Error");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading Role Management: {ex.Message}", "Error");
                }
            }
            else if (_authService != null)
            {
                MessageBox.Show("You do not have permission to access Role Management.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}