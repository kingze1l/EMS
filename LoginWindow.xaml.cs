using System.Windows;
using System.Windows.Controls;
using EMS.Services;
using System;
namespace EMS
{
    public partial class LoginWindow : Window
    {
        private readonly AuthenticationService _authService;
        private PasswordBox? _passwordBox;
        private TextBox? _usernameTextBox;
        private TextBlock? _errorMessage;

        public LoginWindow(AuthenticationService authService)
        {
            InitializeComponent();
            _authService = authService;

            // Get references to controls after InitializeComponent  
            _passwordBox = (PasswordBox)FindName("PasswordBox");
            _usernameTextBox = (TextBox)FindName("UsernameTextBox");
            _errorMessage = (TextBlock)FindName("ErrorMessage");

            if (_usernameTextBox != null)
            {
                _usernameTextBox.Focus();
            }
        }

        public LoginWindow() : this(new AuthenticationService(MongoDbContext.CreateFromConfig()))
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
                    var mainWindow = new MainWindow(_authService);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
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