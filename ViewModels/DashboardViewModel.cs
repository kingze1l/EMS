using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EMS.Models;
using EMS.Services;
using LiveCharts;
using LiveCharts.Wpf;

namespace EMS.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly EmployeeService _employeeService;
        private readonly LeaveService _leaveService;
        private readonly PayrollService _payrollService;
        private readonly AuthenticationService _authService;

        // Statistics
        private int _totalEmployees;
        private int _activeAdmins;
        private int _pendingLeaves;
        private decimal _recentPayrollTotal;
        private bool _isAdmin;

        // Date Range
        private DateTime _startDate = DateTime.Now.AddDays(-30);
        private DateTime _endDate = DateTime.Now;

        // Charts
        private SeriesCollection _roleDistributionChart;
        private SeriesCollection _leaveStatusChart;
        private SeriesCollection _payrollTrendChart;

        // Recent Activity
        private ObservableCollection<PayrollRecord> _recentPayrollRecords;
        private ObservableCollection<LeaveRequest> _recentLeaveRequests;

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand DateRangeChangedCommand { get; }

        public DashboardViewModel(
            EmployeeService employeeService,
            LeaveService leaveService,
            PayrollService payrollService,
            AuthenticationService authService)
        {
            _employeeService = employeeService;
            _leaveService = leaveService;
            _payrollService = payrollService;
            _authService = authService;

            RefreshCommand = new RelayCommand(async () => await LoadDashboardDataAsync());
            DateRangeChangedCommand = new RelayCommand(async () => await LoadDashboardDataAsync());

            _recentPayrollRecords = new ObservableCollection<PayrollRecord>();
            _recentLeaveRequests = new ObservableCollection<LeaveRequest>();

            InitializeCharts();
            _ = LoadDashboardDataAsync();
        }

        #region Properties

        public int TotalEmployees
        {
            get => _totalEmployees;
            set => SetProperty(ref _totalEmployees, value);
        }

        public int ActiveAdmins
        {
            get => _activeAdmins;
            set => SetProperty(ref _activeAdmins, value);
        }

        public int PendingLeaves
        {
            get => _pendingLeaves;
            set => SetProperty(ref _pendingLeaves, value);
        }

        public decimal RecentPayrollTotal
        {
            get => _recentPayrollTotal;
            set => SetProperty(ref _recentPayrollTotal, value);
        }

        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public SeriesCollection RoleDistributionChart
        {
            get => _roleDistributionChart;
            set => SetProperty(ref _roleDistributionChart, value);
        }

        public SeriesCollection LeaveStatusChart
        {
            get => _leaveStatusChart;
            set => SetProperty(ref _leaveStatusChart, value);
        }

        public SeriesCollection PayrollTrendChart
        {
            get => _payrollTrendChart;
            set => SetProperty(ref _payrollTrendChart, value);
        }

        public ObservableCollection<PayrollRecord> RecentPayrollRecords
        {
            get => _recentPayrollRecords;
            set => SetProperty(ref _recentPayrollRecords, value);
        }

        public ObservableCollection<LeaveRequest> RecentLeaveRequests
        {
            get => _recentLeaveRequests;
            set => SetProperty(ref _recentLeaveRequests, value);
        }

        #endregion

        private void InitializeCharts()
        {
            RoleDistributionChart = new SeriesCollection();
            LeaveStatusChart = new SeriesCollection();
            PayrollTrendChart = new SeriesCollection();
        }

        public async Task LoadDashboardDataAsync()
        {
            try
            {
                IsAdmin = _authService.IsUserInRole("Admin");

                // Load basic statistics
                await LoadStatisticsAsync();

                // Load charts
                await LoadRoleDistributionChartAsync();
                await LoadLeaveStatusChartAsync();
                await LoadPayrollTrendChartAsync();

                // Load recent activity
                await LoadRecentActivityAsync();
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
        }

        private async Task LoadStatisticsAsync()
        {
            var employees = await _employeeService.GetAllEmployeesAsync(_authService.CurrentUser?.UserRole ?? new UserRole { RoleName = "Employee" });
            var leaveRequests = await _leaveService.GetLeaveRequestsAsync();
            var payrollRecords = await _payrollService.GetPayrollRecordsByDateRangeAsync(StartDate, EndDate);

            TotalEmployees = employees.Count();
            ActiveAdmins = employees.Count(e => e.UserRole.RoleName == "Admin");
            PendingLeaves = leaveRequests.Count(l => l.Status == LeaveStatus.Pending);
            RecentPayrollTotal = payrollRecords.Sum(p => p.NetPay);
        }

        private async Task LoadRoleDistributionChartAsync()
        {
            var employees = await _employeeService.GetAllEmployeesAsync(_authService.CurrentUser?.UserRole ?? new UserRole { RoleName = "Employee" });
            var roleGroups = employees.GroupBy(e => e.UserRole.RoleName).ToList();

            var series = new PieSeries
            {
                Title = "Role Distribution",
                Values = new ChartValues<int>(roleGroups.Select(g => g.Count())),
                DataLabels = true,
                LabelPoint = point => $"{roleGroups[(int)point.Participation].Key}: {point.Participation:P0}"
            };

            RoleDistributionChart.Clear();
            RoleDistributionChart.Add(series);
        }

        private async Task LoadLeaveStatusChartAsync()
        {
            var leaveRequests = await _leaveService.GetLeaveRequestsAsync();
            var statusGroups = leaveRequests.GroupBy(l => l.Status).ToList();

            var series = new ColumnSeries
            {
                Title = "Leave Status",
                Values = new ChartValues<int>(statusGroups.Select(g => g.Count())),
                DataLabels = true
            };

            LeaveStatusChart.Clear();
            LeaveStatusChart.Add(series);
        }

        private async Task LoadPayrollTrendChartAsync()
        {
            if (!IsAdmin) return;

            var payrollRecords = await _payrollService.GetPayrollRecordsByDateRangeAsync(StartDate, EndDate);
            var monthlyGroups = payrollRecords
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ToList();

            var series = new LineSeries
            {
                Title = "Monthly Payroll Total",
                Values = new ChartValues<decimal>(monthlyGroups.Select(g => g.Sum(p => p.NetPay))),
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 8
            };

            PayrollTrendChart.Clear();
            PayrollTrendChart.Add(series);
        }

        private async Task LoadRecentActivityAsync()
        {
            var payrollRecords = await _payrollService.GetPayrollRecordsByDateRangeAsync(StartDate, EndDate);
            var leaveRequests = await _leaveService.GetLeaveRequestsAsync();

            RecentPayrollRecords.Clear();
            foreach (var record in payrollRecords.Take(5))
            {
                RecentPayrollRecords.Add(record);
            }

            RecentLeaveRequests.Clear();
            foreach (var request in leaveRequests.OrderByDescending(l => l.RequestedDate).Take(5))
            {
                RecentLeaveRequests.Add(request);
            }
        }

        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
} 