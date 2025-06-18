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

            ShowLoading(true);

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
            finally
            {
                ShowLoading(false);
            }
        }

        private async void BiometricButton_Click(object sender, RoutedEventArgs e)
        {
            if (_errorMessage != null)
            {
                _errorMessage.Visibility = Visibility.Collapsed;
            }

            ShowLoading(true);

            try
            {
                // Check if biometric authentication is available
                bool isBiometricAvailable = await _authService.IsBiometricAvailableAsync();
                
                if (!isBiometricAvailable)
                {
                    ShowError("Biometric authentication is not available on this device. Please use username/password login.");
                    return;
                }

                // Attempt biometric authentication
                bool isAuthenticated = await _authService.LoginWithBiometricAsync();

                if (isAuthenticated)
                {
                    // Log successful biometric login
                    await _auditLogService.LogActionAsync(new AuditLog
                    {
                        UserId = _authService.CurrentUser?.Id ?? "unknown",
                        UserName = "samiullah",
                        Action = "BiometricLogin",
                        EntityType = "User",
                        EntityId = _authService.CurrentUser?.Id ?? "unknown",
                        Details = "Master admin logged in with biometric authentication",
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
                    // Log failed biometric attempt
                    await _auditLogService.LogActionAsync(new AuditLog
                    {
                        UserId = "unknown",
                        UserName = "samiullah",
                        Action = "BiometricLoginFailed",
                        EntityType = "User",
                        EntityId = "unknown",
                        Details = "Failed biometric authentication attempt",
                        IpAddress = NetworkUtils.GetLocalIpAddress()
                    });

                    ShowError("Biometric authentication failed. Please try again or use username/password login.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Biometric authentication failed: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowLoading(bool show)
        {
            var loadingIndicator = FindName("LoadingIndicator") as StackPanel;
            var loginForm = FindName("LoginForm") as StackPanel;
            var loginButton = FindName("LoginButton") as Button;
            var biometricButton = FindName("BiometricButton") as Button;

            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            }

            if (loginForm != null)
            {
                loginForm.Opacity = show ? 0.5 : 1.0;
            }

            if (loginButton != null)
            {
                loginButton.IsEnabled = !show;
            }

            if (biometricButton != null)
            {
                biometricButton.IsEnabled = !show;
            }
        }
    }
} 