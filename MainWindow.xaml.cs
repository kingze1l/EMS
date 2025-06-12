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
using System.Windows.Threading;
using System.Threading.Tasks;
using EMS.Utils;

namespace EMS
{
    public partial class MainWindow : Window
    {
        private readonly AuthenticationService? _authService;
        private readonly IEmployeeService? _employeeService;
        private readonly ISettingsService _settingsService;
        private readonly IRoleService _roleService;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;
        private Frame? _mainFrame;
        private readonly DispatcherTimer _notificationUpdateTimer;

        public MainWindow(
            AuthenticationService authService,
            IEmployeeService employeeService,
            ISettingsService settingsService,
            IRoleService roleService,
            INotificationService notificationService,
            IAuditLogService auditLogService)
        {
            InitializeComponent();
            _authService = authService;
            _employeeService = employeeService;
            _settingsService = settingsService;
            _roleService = roleService;
            _notificationService = notificationService;
            _auditLogService = auditLogService;
            
            var app = Application.Current as App;
            if (app?.ServiceProvider != null)
            {
                _employeeService = app.ServiceProvider.GetService<IEmployeeService>();
            }
            
            _mainFrame = FindName("MainFrame") as Frame;
            
            // Set up notification update timer
            _notificationUpdateTimer = new DispatcherTimer();
            _notificationUpdateTimer.Interval = TimeSpan.FromSeconds(30);
            _notificationUpdateTimer.Tick += async (s, e) => await UpdateNotificationCount();
            _notificationUpdateTimer.Start();

            // Set up notifications and update UI for current user
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
                    var app = Application.Current as App;
                    if (app?.ServiceProvider != null)
                    {
                        var roleService = app.ServiceProvider.GetRequiredService<IRoleService>();
                        var auditLogService = app.ServiceProvider.GetRequiredService<IAuditLogService>();

                        _mainFrame.Content = new EmployeeView(
                            _employeeService,
                            roleService,
                            auditLogService,
                            _authService.CurrentUser.UserRole
                        );
                    }
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
                if (_mainFrame != null && _authService.CurrentUser != null)
                {
                    try
                    {
                        var app = Application.Current as App;
                        var settingsView = app?.ServiceProvider.GetRequiredService<SettingsView>();
                        if (settingsView != null)
                        {
                            var viewModel = new EMS.ViewModels.SettingsViewModel(
                                _settingsService,
                                _authService.CurrentUser.Id,
                                _authService.IsUserInRole("Admin")
                            );
                            settingsView.DataContext = viewModel;
                            _mainFrame.Content = settingsView;
                        }
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
            else if (_authService != null)
            {
                MessageBox.Show("You do not have permission to view Settings.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AuditLogs_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AuditLogView(_auditLogService));
        }

        private void LeaveRequests_Click(object sender, RoutedEventArgs e)
        {
            if (_mainFrame != null && _authService?.CurrentUser != null)
            {
                var app = Application.Current as App;
                if (app?.ServiceProvider != null)
                {
                    var leaveService = app.ServiceProvider.GetRequiredService<ILeaveService>();
                    var auditLogService = app.ServiceProvider.GetRequiredService<IAuditLogService>();
                    var employeeService = app.ServiceProvider.GetRequiredService<IEmployeeService>();
                    var currentUser = _authService.CurrentUser;
                    var currentUserRole = currentUser.UserRole;

                    _mainFrame.Content = new LeaveRequestView(
                        leaveService,
                        auditLogService,
                        employeeService,
                        currentUser.Id,
                        currentUserRole
                    );
                }
            }
            else
            {
                MessageBox.Show("User not authenticated or services not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Logout", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Log the logout action
                if (_authService?.CurrentUser != null)
                {
                    _auditLogService.LogActionAsync(new AuditLog
                    {
                        UserId = _authService.CurrentUser.Id,
                        UserName = _authService.CurrentUser.Name,
                        Action = "Logout",
                        EntityType = "User",
                        EntityId = _authService.CurrentUser.Id,
                        Details = "User logged out",
                        IpAddress = NetworkUtils.GetLocalIpAddress()
                    }).ConfigureAwait(false);
                }

                // Create and show new login window
                var loginWindow = new LoginWindow(
                    _authService!,
                    _employeeService!,
                    _settingsService,
                    _roleService,
                    _notificationService,
                    _auditLogService);
                loginWindow.Show();
                
                // Close current window
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

        private async Task UpdateNotificationCount()
        {
            if (_authService?.CurrentUser == null) return;

            try
            {
                var notificationService = ((App)Application.Current).ServiceProvider.GetRequiredService<INotificationService>();
                var unreadCount = await notificationService.GetUnreadCountAsync(_authService.CurrentUser.Id);
                
                if (FindName("NotificationButton") is Button notificationButton)
                {
                    notificationButton.Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new TextBlock { Text = "🔔", Margin = new Thickness(0, 0, 5, 0) },
                            new TextBlock { Text = unreadCount > 0 ? unreadCount.ToString() : "", 
                                          Foreground = System.Windows.Media.Brushes.Red,
                                          FontWeight = FontWeights.Bold }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                // Log error but don't show to user
                System.Diagnostics.Debug.WriteLine($"Error updating notification count: {ex.Message}");
            }
        }

        private void Notification_Click(object sender, RoutedEventArgs e)
        {
            if (_authService?.CurrentUser == null)
            {
                MessageBox.Show("Please log in to view notifications.", "Error");
                return;
            }

            if (_mainFrame != null)
            {
                var notificationService = ((App)Application.Current).ServiceProvider.GetRequiredService<INotificationService>();
                var notificationView = new NotificationView(notificationService, _authService.CurrentUser.Id);
                _mainFrame.Content = notificationView;
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

        private async void TestNotification_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_authService?.CurrentUser == null)
                {
                    MessageBox.Show("No user is currently logged in.", "Error");
                    return;
                }

                var notificationService = ((App)Application.Current).ServiceProvider.GetRequiredService<INotificationService>();
                
                var notification = new Notification
                {
                    Title = "Test Notification",
                    Message = "This is a test notification",
                    Type = "Info",
                    UserId = _authService.CurrentUser.Id,
                    ActionUrl = "/employees",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                await notificationService.CreateNotificationAsync(notification);

                // Log the test notification creation
                await _auditLogService.LogActionAsync(new AuditLog
                {
                    UserId = _authService.CurrentUser.Id,
                    UserName = _authService.CurrentUser.Name,
                    Action = "Create",
                    EntityType = "Notification",
                    EntityId = notification.Id,
                    Details = "Created test notification",
                    IpAddress = NetworkUtils.GetLocalIpAddress()
                });

                MessageBox.Show("Test notification created successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating test notification: {ex.Message}");
            }
        }

        private void Payroll_Click(object sender, RoutedEventArgs e)
        {
            if (_authService != null && _authService.HasPermission(EMS.Models.Permission.ViewPayroll))
            {
                if (_mainFrame != null && _authService.CurrentUser != null)
                {
                    var app = Application.Current as App;
                    if (app?.ServiceProvider != null)
                    {
                        var payrollService = app.ServiceProvider.GetRequiredService<IPayrollService>();
                        var employeeService = app.ServiceProvider.GetRequiredService<IEmployeeService>();
                        var auditLogService = app.ServiceProvider.GetRequiredService<IAuditLogService>();
                        var payrollView = app.ServiceProvider.GetRequiredService<EMS.Views.PayrollView>();
                        var viewModel = new EMS.ViewModels.PayrollViewModel(
                            payrollService,
                            employeeService,
                            auditLogService,
                            _authService.CurrentUser
                        );
                        payrollView.DataContext = viewModel;
                        _mainFrame.Content = payrollView;
                    }
                }
            }
            else if (_authService != null)
            {
                MessageBox.Show("You do not have permission to view Payroll.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}