using System.Windows;
using System.Windows.Controls;
using EMS.Services;
using EMS.Models;
using System;
using EMS.Utils;

namespace EMS
{
    public partial class LoginWindow : Window
    {
        private readonly AuthenticationService _authService;
        private readonly IEmployeeService _employeeService;
        private readonly ISettingsService _settingsService;
        private readonly IRoleService _roleService;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;
        private PasswordBox? _passwordBox;
        private TextBox? _usernameTextBox;
        private TextBlock? _errorMessage;

        public LoginWindow(
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

            // Get references to controls after InitializeComponent  
            _passwordBox = (PasswordBox)FindName("PasswordBox");
            _usernameTextBox = (TextBox)FindName("UsernameTextBox");
            _errorMessage = (TextBlock)FindName("ErrorMessage");

            if (_usernameTextBox != null)
            {
                _usernameTextBox.Focus();
            }
        }

        public LoginWindow() : this(
            new AuthenticationService(MongoDbContext.CreateFromConfig()),
            new EmployeeService(MongoDbContext.CreateFromConfig()),
            new SettingsService(MongoDbContext.CreateFromConfig().Database),
            new RoleService(MongoDbContext.CreateFromConfig().Database),
            new NotificationService(MongoDbContext.CreateFromConfig().Database),
            new AuditLogService(MongoDbContext.CreateFromConfig().Database))
        {
            // For XAML designer and default instantiation
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_errorMessage != null)
            {
                _errorMessage.Visibility = Visibility.Collapsed;
            }

            if (_usernameTextBox == null || _passwordBox == null)
            {
                ShowError("UI controls not properly initialized.");
                return;
            }

            string username = _usernameTextBox.Text;
            string password = _passwordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter both username and password.");
                return;
            }

            try
            {
                bool isAuthenticated = await _authService.LoginAsync(username, password);

                if (isAuthenticated)
                {
                    // Log successful login
                    await _auditLogService.LogActionAsync(new AuditLog
                    {
                        UserId = _authService.CurrentUser?.Id ?? "unknown",
                        UserName = username,
                        Action = "Login",
                        EntityType = "User",
                        EntityId = _authService.CurrentUser?.Id ?? "unknown",
                        Details = "User logged in successfully",
                        IpAddress = NetworkUtils.GetLocalIpAddress()
                    });

                    var mainWindow = new MainWindow(
                        _authService,
                        _employeeService,
                        _settingsService,
                        _roleService,
                        _notificationService,
                        _auditLogService);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    // Log failed login attempt
                    await _auditLogService.LogActionAsync(new AuditLog
                    {
                        UserId = "unknown",
                        UserName = username,
                        Action = "LoginFailed",
                        EntityType = "User",
                        EntityId = "unknown",
                        Details = "Failed login attempt",
                        IpAddress = NetworkUtils.GetLocalIpAddress()
                    });

                    ShowError("Invalid username or password.");
                    if (_passwordBox != null)
                    {
                        _passwordBox.Password = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Login failed: {ex.Message}");
            }
        }

        private async void FingerprintButton_Click(object sender, RoutedEventArgs e)
        {
            if (_errorMessage != null)
            {
                _errorMessage.Visibility = Visibility.Collapsed;
            }

            try
            {
                // Check if fingerprint is available
                bool isFingerprintAvailable = await _authService.IsFingerprintAvailableAsync();
                
                if (!isFingerprintAvailable)
                {
                    ShowError("Fingerprint authentication is not available on this device. Please use username/password login.");
                    return;
                }

                // Attempt fingerprint authentication
                bool isAuthenticated = await _authService.LoginWithFingerprintAsync();

                if (isAuthenticated)
                {
                    // Log successful fingerprint login
                    await _auditLogService.LogActionAsync(new AuditLog
                    {
                        UserId = _authService.CurrentUser?.Id ?? "unknown",
                        UserName = "samiullah",
                        Action = "FingerprintLogin",
                        EntityType = "User",
                        EntityId = _authService.CurrentUser?.Id ?? "unknown",
                        Details = "Master admin logged in with fingerprint",
                        IpAddress = NetworkUtils.GetLocalIpAddress()
                    });

                    var mainWindow = new MainWindow(
                        _authService,
                        _employeeService,
                        _settingsService,
                        _roleService,
                        _notificationService,
                        _auditLogService);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    // Log failed fingerprint attempt
                    await _auditLogService.LogActionAsync(new AuditLog
                    {
                        UserId = "unknown",
                        UserName = "samiullah",
                        Action = "FingerprintLoginFailed",
                        EntityType = "User",
                        EntityId = "unknown",
                        Details = "Failed fingerprint authentication attempt",
                        IpAddress = NetworkUtils.GetLocalIpAddress()
                    });

                    ShowError("Fingerprint authentication failed. Please try again or use username/password login.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Fingerprint authentication failed: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            if (_errorMessage != null)
            {
                _errorMessage.Text = message;
                _errorMessage.Visibility = Visibility.Visible;
            }
        }
    }
} 