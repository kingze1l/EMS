using EMS.Models;
using EMS.Services;
using EMS.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EMS.Views;

namespace EMS.ViewModels
{
    public class PayrollViewModel : ViewModelBase
    {
        private readonly IPayrollService _payrollService;
        private readonly IEmployeeService _employeeService;
        private readonly IAuditLogService _auditLogService;
        private readonly Employee _currentUser;

        private ObservableCollection<PayrollRecord> _payrollRecords;
        private ObservableCollection<Employee> _employees;
        private PayrollRecord? _selectedPayrollRecord;
        private Employee? _selectedEmployee;
        private string _searchText = string.Empty;
        private DateTime _startDate = DateTime.Now.AddMonths(-1);
        private DateTime _endDate = DateTime.Now;
        private decimal _totalPayroll;
        private bool _isLoading;
        private string _statusMessage = string.Empty;

        public PayrollViewModel(IPayrollService payrollService, IEmployeeService employeeService, 
            IAuditLogService auditLogService, Employee currentUser)
        {
            _payrollService = payrollService ?? throw new ArgumentNullException(nameof(payrollService));
            _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            _payrollRecords = new ObservableCollection<PayrollRecord>();
            _employees = new ObservableCollection<Employee>();

            LoadPayrollRecordsCommand = new RelayCommand(async () => await LoadPayrollRecordsAsync());
            LoadEmployeesCommand = new RelayCommand(async () => await LoadEmployeesAsync());
            UpdateEmployeePayCommand = new RelayCommand(async () => await UpdateEmployeePayAsync());
            GeneratePayrollCommand = new RelayCommand(async () => await GeneratePayrollAsync());
            GenerateBulkPayrollCommand = new RelayCommand(async () => await GenerateBulkPayrollAsync());
            SearchCommand = new RelayCommand(async () => await SearchPayrollRecordsAsync());
            CalculateTotalCommand = new RelayCommand(async () => await CalculateTotalPayrollAsync());

            // Load initial data
            _ = LoadEmployeesAsync();
            _ = LoadPayrollRecordsAsync();
        }

        public ObservableCollection<PayrollRecord> PayrollRecords
        {
            get => _payrollRecords;
            set
            {
                _payrollRecords = value;
                OnPropertyChanged();
            }
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

        public PayrollRecord? SelectedPayrollRecord
        {
            get => _selectedPayrollRecord;
            set
            {
                _selectedPayrollRecord = value;
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
                if (value != null)
                {
                    // Update the pay details when an employee is selected
                    OnPropertyChanged(nameof(SelectedEmployeeBasePay));
                    OnPropertyChanged(nameof(SelectedEmployeeBonus));
                    OnPropertyChanged(nameof(SelectedEmployeeDeductions));
                    OnPropertyChanged(nameof(SelectedEmployeeNetPay));
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalPayroll
        {
            get => _totalPayroll;
            set
            {
                _totalPayroll = value;
                OnPropertyChanged();
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

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        // Properties for editing employee pay details
        public decimal SelectedEmployeeBasePay
        {
            get => SelectedEmployee?.BasePay ?? 0;
            set
            {
                if (SelectedEmployee != null)
                {
                    SelectedEmployee.BasePay = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedEmployeeNetPay));
                }
            }
        }

        public decimal SelectedEmployeeBonus
        {
            get => SelectedEmployee?.Bonus ?? 0;
            set
            {
                if (SelectedEmployee != null)
                {
                    SelectedEmployee.Bonus = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedEmployeeNetPay));
                }
            }
        }

        public decimal SelectedEmployeeDeductions
        {
            get => SelectedEmployee?.Deductions ?? 0;
            set
            {
                if (SelectedEmployee != null)
                {
                    SelectedEmployee.Deductions = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedEmployeeNetPay));
                }
            }
        }

        public decimal SelectedEmployeeNetPay
        {
            get => SelectedEmployee != null ? 
                SelectedEmployee.BasePay + SelectedEmployee.Bonus - SelectedEmployee.Deductions : 0;
        }

        // Commands
        public ICommand LoadPayrollRecordsCommand { get; }
        public ICommand LoadEmployeesCommand { get; }
        public ICommand UpdateEmployeePayCommand { get; }
        public ICommand GeneratePayrollCommand { get; }
        public ICommand GenerateBulkPayrollCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CalculateTotalCommand { get; }

        private async Task LoadPayrollRecordsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading payroll records...";

                var records = await _payrollService.GetAllPayrollRecordsAsync();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PayrollRecords.Clear();
                    foreach (var record in records)
                    {
                        PayrollRecords.Add(record);
                    }
                });

                StatusMessage = $"Loaded {PayrollRecords.Count} payroll records.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading payroll records: {ex.Message}";
                NotificationPopup.ShowPopup(StatusMessage, NotificationType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadEmployeesAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading employees...";

                var employees = await _employeeService.GetAllEmployeesAsync(_currentUser.UserRole);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Employees.Clear();
                    foreach (var employee in employees)
                    {
                        Employees.Add(employee);
                    }
                });

                StatusMessage = $"Loaded {Employees.Count} employees.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading employees: {ex.Message}";
                NotificationPopup.ShowPopup(StatusMessage, NotificationType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateEmployeePayAsync()
        {
            if (SelectedEmployee == null)
            {
                NotificationPopup.ShowPopup("Please select an employee to update.", NotificationType.Info);
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Updating employee pay details...";

                var success = await _payrollService.UpdateEmployeePayDetailsAsync(
                    SelectedEmployee.Id,
                    SelectedEmployee.BasePay,
                    SelectedEmployee.Bonus,
                    SelectedEmployee.Deductions);

                if (success)
                {
                    // Log the change
                    await _auditLogService.LogActionAsync(new AuditLog
                    {
                        UserId = _currentUser.Id,
                        UserName = _currentUser.Name,
                        Action = "Update",
                        EntityType = "Employee",
                        EntityId = SelectedEmployee.Id,
                        Details = $"Updated pay details for {SelectedEmployee.Name}: Base={SelectedEmployee.BasePay}, Bonus={SelectedEmployee.Bonus}, Deductions={SelectedEmployee.Deductions}",
                        Timestamp = DateTime.UtcNow,
                        IpAddress = "127.0.0.1"
                    });

                    StatusMessage = $"Successfully updated pay details for {SelectedEmployee.Name}.";
                    NotificationPopup.ShowPopup(StatusMessage, NotificationType.Success);
                }
                else
                {
                    StatusMessage = "Failed to update employee pay details.";
                    NotificationPopup.ShowPopup(StatusMessage, NotificationType.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating employee pay: {ex.Message}";
                NotificationPopup.ShowPopup(StatusMessage, NotificationType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GeneratePayrollAsync()
        {
            if (SelectedEmployee == null)
            {
                NotificationPopup.ShowPopup("Please select an employee to generate payroll for.", NotificationType.Info);
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"Generating payroll for {SelectedEmployee.Name}...";

                // Log the action
                var auditLog = await _auditLogService.LogActionAsync(new AuditLog
                {
                    UserId = _currentUser.Id,
                    UserName = _currentUser.Name,
                    Action = "Generate",
                    EntityType = "Payroll",
                    EntityId = SelectedEmployee.Id,
                    Details = $"Generated payroll record for {SelectedEmployee.Name}",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = "127.0.0.1"
                });

                var success = await _payrollService.GeneratePayrollRecordAsync(SelectedEmployee.Id, auditLog.Id);

                if (success)
                {
                    StatusMessage = $"Successfully generated payroll for {SelectedEmployee.Name}.";
                    NotificationPopup.ShowPopup(StatusMessage, NotificationType.Success);
                    
                    // Refresh the payroll records
                    await LoadPayrollRecordsAsync();
                }
                else
                {
                    StatusMessage = "Failed to generate payroll record.";
                    NotificationPopup.ShowPopup(StatusMessage, NotificationType.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error generating payroll: {ex.Message}";
                NotificationPopup.ShowPopup(StatusMessage, NotificationType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateBulkPayrollAsync()
        {
            if (!Employees.Any())
            {
                NotificationPopup.ShowPopup("No employees available for bulk payroll generation.", NotificationType.Info);
                return;
            }

            var result = MessageBox.Show(
                $"Generate payroll for all {Employees.Count} employees?", 
                "Bulk Payroll Generation", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;
                StatusMessage = "Generating bulk payroll...";

                var employeeIds = Employees.Select(e => e.Id).ToList();

                // Log the action
                var auditLog = await _auditLogService.LogActionAsync(new AuditLog
                {
                    UserId = _currentUser.Id,
                    UserName = _currentUser.Name,
                    Action = "Generate",
                    EntityType = "Payroll",
                    EntityId = "BULK",
                    Details = $"Generated bulk payroll for {employeeIds.Count} employees",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = "127.0.0.1"
                });

                var success = await _payrollService.BulkGeneratePayrollAsync(employeeIds, auditLog.Id);

                if (success)
                {
                    StatusMessage = $"Successfully generated payroll for {employeeIds.Count} employees.";
                    NotificationPopup.ShowPopup(StatusMessage, NotificationType.Success);
                    
                    // Refresh the payroll records
                    await LoadPayrollRecordsAsync();
                }
                else
                {
                    StatusMessage = "Failed to generate bulk payroll.";
                    NotificationPopup.ShowPopup(StatusMessage, NotificationType.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error generating bulk payroll: {ex.Message}";
                NotificationPopup.ShowPopup(StatusMessage, NotificationType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchPayrollRecordsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Searching payroll records...";

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await LoadPayrollRecordsAsync();
                    return;
                }

                var records = await _payrollService.GetPayrollRecordsByEmployeeNameAsync(SearchText);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PayrollRecords.Clear();
                    foreach (var record in records)
                    {
                        PayrollRecords.Add(record);
                    }
                });

                StatusMessage = $"Found {PayrollRecords.Count} payroll records for '{SearchText}'.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error searching payroll records: {ex.Message}";
                NotificationPopup.ShowPopup(StatusMessage, NotificationType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CalculateTotalPayrollAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Calculating total payroll...";

                var total = await _payrollService.GetTotalPayrollForPeriodAsync(StartDate, EndDate);
                TotalPayroll = total;

                StatusMessage = $"Total payroll for period: ${TotalPayroll:F2}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error calculating total payroll: {ex.Message}";
                NotificationPopup.ShowPopup(StatusMessage, NotificationType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 