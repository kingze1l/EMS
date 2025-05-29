using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using EMS.Models;
using EMS.Services;
using System.Windows;
using System.Linq;
using System.Collections.Generic;

namespace EMS.ViewModels
{
    public class EmployeeViewModel : INotifyPropertyChanged
    {
        private readonly IEmployeeService _employeeService;
        private readonly IRoleService _roleService;
        private readonly UserRole _currentUserRole;
        private ObservableCollection<Employee> _employees;
        private Employee? _selectedEmployee;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private ObservableCollection<Role> _availableRoles;
        private Role? _selectedRoleForEmployee;
        private bool _isEditingOrAdding;

        public EmployeeViewModel(IEmployeeService employeeService, IRoleService roleService, UserRole currentUserRole)
        {
            _employeeService = employeeService;
            _roleService = roleService;
            _currentUserRole = currentUserRole;
            _employees = new ObservableCollection<Employee>();
            _availableRoles = new ObservableCollection<Role>();
            
            // Initialize commands
            AddEmployeeCommand = new RelayCommand(AddEmployee);
            UpdateEmployeeCommand = new RelayCommand(EditEmployee, () => SelectedEmployee != null);
            DeleteEmployeeCommand = new RelayCommand(async () => await DeleteEmployee(), () => SelectedEmployee != null);
            SearchCommand = new RelayCommand(async () => await SearchEmployees());
            RefreshCommand = new RelayCommand(async () => await LoadEmployees());
            ResetPasswordCommand = new RelayCommand(async () => await ResetPassword(), () => SelectedEmployee != null);
            SaveEmployeeCommand = new RelayCommand(async () => await SaveEmployee(), () => SelectedEmployee != null && SelectedRoleForEmployee != null && !IsLoading);
            CancelEditCommand = new RelayCommand(CancelEdit);

            // Load data
            _ = LoadEmployees();
            _ = LoadAvailableRoles();
        }

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set
            {
                _employees = value;
                OnPropertyChanged();
            }
        }

        public Employee? SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged();
                SelectedRoleForEmployee = _selectedEmployee?.UserRole != null ? 
                                          AvailableRoles.FirstOrDefault(r => r.RoleName == _selectedEmployee.UserRole.RoleName) : null;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                if (string.IsNullOrWhiteSpace(value))
                {
                    _ = LoadEmployees();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ObservableCollection<Role> AvailableRoles
        {
            get => _availableRoles;
            set
            {
                _availableRoles = value;
                OnPropertyChanged();
            }
        }

        public Role? SelectedRoleForEmployee
        {
            get => _selectedRoleForEmployee;
            set
            {
                _selectedRoleForEmployee = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsEditingOrAdding
        {
            get => _isEditingOrAdding;
            set
            {
                _isEditingOrAdding = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddEmployeeCommand { get; }
        public ICommand UpdateEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand SaveEmployeeCommand { get; }
        public ICommand CancelEditCommand { get; }

        private async Task LoadEmployees()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var employees = await _employeeService.GetAllEmployeesAsync(_currentUserRole);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Employees.Clear();
                    foreach (var employee in employees)
                    {
                        Employees.Add(employee);
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading employees: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadAvailableRoles()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableRoles.Clear();
                    foreach (var role in roles)
                    {
                        AvailableRoles.Add(role);
                    }
                    SelectedRoleForEmployee = AvailableRoles.FirstOrDefault(r => r.RoleName == "Employee") ?? AvailableRoles.FirstOrDefault();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading roles: {ex.Message}");
            }
        }

        private async Task SearchEmployees()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadEmployees();
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                var results = await _employeeService.SearchEmployeesAsync(SearchText, _currentUserRole);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Employees.Clear();
                    foreach (var employee in results)
                    {
                        Employees.Add(employee);
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error searching employees: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddEmployee()
        {
            SelectedEmployee = new Employee
            {
                Name = string.Empty,
                Position = string.Empty,
                Contact = string.Empty,
                Username = string.Empty,
                Password = string.Empty,
                UserRole = new UserRole { RoleID = 0, RoleName = string.Empty },
                DateOfBirth = DateTime.Now
            };
            SelectedRoleForEmployee = AvailableRoles.FirstOrDefault(r => r.RoleName == "Employee") ?? AvailableRoles.FirstOrDefault();
            if (SelectedEmployee.UserRole != null && SelectedRoleForEmployee != null)
            {
                SelectedEmployee.UserRole.RoleName = SelectedRoleForEmployee.RoleName;
            }
            IsEditingOrAdding = true;
            ErrorMessage = string.Empty;
        }

        private void EditEmployee()
        {
            if (SelectedEmployee == null) return;

            SelectedRoleForEmployee = AvailableRoles.FirstOrDefault(r => r.RoleName == SelectedEmployee.UserRole?.RoleName);

            IsEditingOrAdding = true;
            ErrorMessage = string.Empty;
        }

        private async Task SaveEmployee()
        {
            if (SelectedEmployee == null || SelectedRoleForEmployee == null) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                SelectedEmployee.UserRole = new UserRole
                {
                    RoleID = 0,
                    RoleName = SelectedRoleForEmployee.RoleName,
                    Permissions = ConvertPermissionStringsToEnums(SelectedRoleForEmployee.Permissions)
                };

                bool success;
                if (string.IsNullOrEmpty(SelectedEmployee.Id))
                {
                    SelectedEmployee.Password = BCrypt.Net.BCrypt.HashPassword(GenerateRandomPassword());

                    if (string.IsNullOrWhiteSpace(SelectedEmployee.Name) || string.IsNullOrWhiteSpace(SelectedEmployee.Position) ||
                        string.IsNullOrWhiteSpace(SelectedEmployee.Contact) || string.IsNullOrWhiteSpace(SelectedEmployee.Username))
                    {
                        ErrorMessage = "Please fill in all required employee details (Name, Position, Contact, Username).";
                        IsLoading = false;
                        return;
                    }

                    success = await _employeeService.AddEmployeeAsync(SelectedEmployee);
                    if (success)
                    {
                        MessageBox.Show("Employee added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    var existingEmployee = await _employeeService.GetEmployeeByIdAsync(SelectedEmployee.Id, _currentUserRole);
                    if (existingEmployee != null)
                    {
                        SelectedEmployee.Password = existingEmployee.Password;
                    }
                    else
                    {
                        ErrorMessage = "Could not find the original employee data to update.";
                        IsLoading = false;
                        return;
                    }
                    success = await _employeeService.UpdateEmployeeAsync(SelectedEmployee, _currentUserRole);
                    if (success)
                    {
                        MessageBox.Show("Employee updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                if (success)
                {
                    await LoadEmployees();
                    CancelEdit();
                }
                else
                {
                    ErrorMessage = "Failed to save employee.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving employee: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit()
        {
            SelectedEmployee = null;
            SelectedRoleForEmployee = null;
            IsEditingOrAdding = false;
            ErrorMessage = string.Empty;
        }

        private async Task DeleteEmployee()
        {
            if (SelectedEmployee == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete employee: {SelectedEmployee.Name}?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                if (await _employeeService.DeleteEmployeeAsync(SelectedEmployee.Id, _currentUserRole))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Employees.Remove(SelectedEmployee);
                        SelectedEmployee = null;
                    });
                    MessageBox.Show("Employee deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ErrorMessage = "Failed to delete employee.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting employee: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ResetPassword()
        {
            if (SelectedEmployee == null) return;

            var result = MessageBox.Show($"Are you sure you want to reset the password for employee: {SelectedEmployee.Username}?", 
                "Confirm Password Reset", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                string newPassword = GenerateRandomPassword();

                SelectedEmployee.Password = newPassword;

                if (await _employeeService.UpdateEmployeeAsync(SelectedEmployee, _currentUserRole))
                {
                    MessageBox.Show($"Password for {SelectedEmployee.Username} reset successfully!\nNew password: {newPassword}", 
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadEmployees();
                }
                else
                {
                    ErrorMessage = $"Failed to reset password for {SelectedEmployee.Username}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error resetting password: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private List<Permission> ConvertPermissionStringsToEnums(List<string> permissionNames)
        {
            var permissions = new List<Permission>();
            if (permissionNames == null) return permissions;

            foreach (var name in permissionNames)
            {
                if (Enum.TryParse(name, out Permission permissionEnum))
                {
                    permissions.Add(permissionEnum);
                }
            }
            return permissions;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 