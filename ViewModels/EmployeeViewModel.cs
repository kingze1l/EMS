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

namespace EMS.ViewModels
{
    public class EmployeeViewModel : INotifyPropertyChanged
    {
        private readonly IEmployeeService _employeeService;
        private readonly UserRole _currentUserRole;
        private ObservableCollection<Employee> _employees;
        private Employee? _selectedEmployee;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public EmployeeViewModel(IEmployeeService employeeService, UserRole currentUserRole)
        {
            _employeeService = employeeService;
            _currentUserRole = currentUserRole;
            _employees = new ObservableCollection<Employee>();
            
            // Initialize commands
            AddEmployeeCommand = new RelayCommand(async () => await AddEmployee());
            UpdateEmployeeCommand = new RelayCommand(async () => await UpdateEmployee(), () => SelectedEmployee != null);
            DeleteEmployeeCommand = new RelayCommand(async () => await DeleteEmployee(), () => SelectedEmployee != null);
            SearchCommand = new RelayCommand(async () => await SearchEmployees());
            RefreshCommand = new RelayCommand(async () => await LoadEmployees());
            ResetPasswordCommand = new RelayCommand(async () => await ResetPassword(), () => SelectedEmployee != null);

            // Load employees
            _ = LoadEmployees();
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
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddEmployeeCommand { get; }
        public ICommand UpdateEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ResetPasswordCommand { get; }

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

        private async Task AddEmployee()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Generate a random password
                string randomPassword = GenerateRandomPassword();

                var newEmployee = new Employee
                {
                    Name = "New Employee",
                    Position = "Position",
                    Contact = "Contact",
                    Username = "username",
                    Password = randomPassword,
                    UserRole = new UserRole { RoleID = 2, RoleName = "Employee" },
                    DateOfBirth = DateTime.Now
                };

                if (await _employeeService.AddEmployeeAsync(newEmployee))
                {
                    await LoadEmployees();
                    MessageBox.Show($"Employee added successfully!\nInitial password: {randomPassword}", 
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ErrorMessage = "Failed to add employee";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding employee: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateEmployee()
        {
            if (SelectedEmployee == null) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                if (await _employeeService.UpdateEmployeeAsync(SelectedEmployee, _currentUserRole))
                {
                    await LoadEmployees();
                    MessageBox.Show("Employee updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ErrorMessage = "Failed to update employee";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating employee: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteEmployee()
        {
            if (SelectedEmployee == null) return;

            var result = MessageBox.Show("Are you sure you want to delete this employee?", "Confirm Delete",
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
                    ErrorMessage = "Failed to delete employee";
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

            var result = MessageBox.Show("Are you sure you want to reset the password for this employee?", 
                "Confirm Password Reset", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Generate a new random password
                string newPassword = GenerateRandomPassword();
                SelectedEmployee.Password = newPassword;

                if (await _employeeService.UpdateEmployeeAsync(SelectedEmployee, _currentUserRole))
                {
                    MessageBox.Show($"Password has been reset successfully!\nNew password: {newPassword}", 
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ErrorMessage = "Failed to reset password";
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
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 