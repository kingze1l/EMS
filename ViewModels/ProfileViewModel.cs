using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using EMS.Models;
using EMS.Services;
using EMS.Views;

namespace EMS.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private readonly IEmployeeService _employeeService;
        private readonly Employee _currentUser;
        private readonly UserRole _currentUserRole;
        private string _contact;
        private SecureString? _oldPassword;
        private SecureString? _newPassword;
        private string _errorMessage;
        private int _passwordStrength;
        private string _passwordStrengthText = string.Empty;
        private string _successMessage = string.Empty;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ProfileViewModel(IEmployeeService employeeService, Employee currentUser)
        {
            _employeeService = employeeService;
            _currentUser = currentUser;
            _currentUserRole = currentUser.UserRole;
            _contact = currentUser.Contact;
            _errorMessage = string.Empty;
            SaveCommand = new RelayCommand(async () => await Save(), () => true);
            CancelCommand = new RelayCommand(Cancel);
        }

        public string Name => _currentUser.Name;
        public string Username => _currentUser.Username;
        public string Role => _currentUserRole.RoleName;
        public string Contact
        {
            get => _contact;
            set { _contact = value; OnPropertyChanged(); }
        }
        public SecureString? OldPassword
        {
            get => _oldPassword;
            set { _oldPassword = value; OnPropertyChanged(); }
        }
        public SecureString? NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged();
                UpdatePasswordStrength();
            }
        }
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }
        public int PasswordStrength
        {
            get => _passwordStrength;
            set { _passwordStrength = value; OnPropertyChanged(); }
        }
        public string PasswordStrengthText
        {
            get => _passwordStrengthText;
            set { _passwordStrengthText = value; OnPropertyChanged(); }
        }
        public string SuccessMessage
        {
            get => _successMessage;
            set { _successMessage = value; OnPropertyChanged(); }
        }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private async Task Save()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            
            // Check if password is being changed
            bool isPasswordChange = NewPassword != null && NewPassword.Length > 0;
            
            // Show confirmation dialog for password changes
            if (isPasswordChange)
            {
                bool confirmed = await ShowPasswordChangeConfirmation();
                if (!confirmed)
                {
                    return; // User cancelled the password change
                }
            }
            
            var updatedEmployee = new Employee
            {
                Id = _currentUser.Id,
                Name = _currentUser.Name,
                Position = _currentUser.Position,
                Contact = this.Contact,
                Username = _currentUser.Username,
                Password = _currentUser.Password, // will update if needed
                UserRole = _currentUser.UserRole,
                DateOfBirth = _currentUser.DateOfBirth,
                BasePay = _currentUser.BasePay,
                Bonus = _currentUser.Bonus,
                Deductions = _currentUser.Deductions,
                BiometricEnabled = _currentUser.BiometricEnabled,
                BiometricUserId = _currentUser.BiometricUserId,
                BiometricEnrolledDate = _currentUser.BiometricEnrolledDate
            };
            string? oldPassword = null;
            
            if (isPasswordChange)
            {
                // If admin, no old password required
                if (_currentUserRole.RoleName == "Admin")
                {
                    updatedEmployee.Password = BCrypt.Net.BCrypt.HashPassword(new System.Net.NetworkCredential(string.Empty, NewPassword).Password);
                }
                else
                {
                    if (OldPassword == null || OldPassword.Length == 0)
                    {
                        ErrorMessage = "Old password is required.";
                        return;
                    }
                    oldPassword = new System.Net.NetworkCredential(string.Empty, OldPassword).Password;
                    updatedEmployee.Password = BCrypt.Net.BCrypt.HashPassword(new System.Net.NetworkCredential(string.Empty, NewPassword).Password);
                }
            }
            var result = await _employeeService.UpdateEmployeeAsync(updatedEmployee, _currentUserRole, oldPassword);
            if (result)
            {
                if (isPasswordChange)
                {
                    SuccessMessage = "Profile and password updated successfully!";
                }
                else
                {
                    SuccessMessage = "Profile updated successfully!";
                }
                ErrorMessage = string.Empty;
                
                // Clear password fields after successful save
                OldPassword = null;
                NewPassword = null;
                PasswordStrength = 0;
                PasswordStrengthText = string.Empty;
                
                // Update the current user's contact info in memory
                _currentUser.Contact = this.Contact;
            }
            else
            {
                ErrorMessage = "Failed to update profile. Check your old password or contact admin.";
                SuccessMessage = string.Empty;
            }
        }

        private async Task<bool> ShowPasswordChangeConfirmation()
        {
            var result = MessageBox.Show(
                "Are you sure you want to change your password? You will need to use the new password for your next login.",
                "Confirm Password Change",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            
            return result == MessageBoxResult.Yes;
        }

        private void Cancel()
        {
            // Optionally reset fields or close dialog
            Contact = _currentUser.Contact;
            OldPassword = null;
            NewPassword = null;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            PasswordStrength = 0;
            PasswordStrengthText = string.Empty;
        }

        private void UpdatePasswordStrength()
        {
            PasswordStrength = 0;
            PasswordStrengthText = string.Empty;
            if (NewPassword == null || NewPassword.Length == 0)
            {
                PasswordStrengthText = "";
                return;
            }
            var pwd = new System.Net.NetworkCredential(string.Empty, NewPassword).Password;
            int score = 0;
            if (pwd.Length >= 8) score++;
            if (System.Text.RegularExpressions.Regex.IsMatch(pwd, "[0-9]")) score++;
            if (System.Text.RegularExpressions.Regex.IsMatch(pwd, "[A-Z]")) score++;
            if (System.Text.RegularExpressions.Regex.IsMatch(pwd, "[^a-zA-Z0-9]")) score++;
            PasswordStrength = score;
            switch (score)
            {
                case 0:
                case 1: PasswordStrengthText = "Weak"; break;
                case 2: PasswordStrengthText = "Fair"; break;
                case 3: PasswordStrengthText = "Good"; break;
                case 4: PasswordStrengthText = "Strong"; break;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 