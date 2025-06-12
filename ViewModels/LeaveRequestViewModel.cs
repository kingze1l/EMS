using EMS.Models;
using EMS.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EMS.ViewModels;

namespace EMS.ViewModels
{
    public class LeaveRequestViewModel : ViewModelBase
    {
        private readonly ILeaveService _leaveService;
        private readonly IAuditLogService _auditLogService;
        private readonly IEmployeeService _employeeService;
        private readonly string _currentUserId;
        private readonly UserRole _currentUserRole;
        private readonly TimeZoneInfo _nzTimeZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");

        private LeaveRequest _newLeaveRequest = new LeaveRequest();
        private ObservableCollection<LeaveRequest> _leaveRequests = new ObservableCollection<LeaveRequest>();
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;

        public LeaveRequest NewLeaveRequest
        {
            get => _newLeaveRequest;
            set
            {
                _newLeaveRequest = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<LeaveRequest> LeaveRequests
        {
            get => _leaveRequests;
            set
            {
                _leaveRequests = value;
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                _successMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand SubmitLeaveCommand { get; }
        public ICommand ApproveLeaveCommand { get; }
        public ICommand RejectLeaveCommand { get; }
        public ICommand LoadLeaveRequestsCommand { get; }
        public ICommand ResetFormCommand { get; }

        public bool CanManageLeaves => _currentUserRole.RoleName == "Admin" || _currentUserRole.RoleName == "Manager" || _currentUserRole.RoleName == "HR";

        public LeaveRequestViewModel(ILeaveService leaveService, IAuditLogService auditLogService, IEmployeeService employeeService, string currentUserId, UserRole currentUserRole)
        {
            _leaveService = leaveService;
            _auditLogService = auditLogService;
            _employeeService = employeeService;
            _currentUserId = currentUserId;
            _currentUserRole = currentUserRole;

            SubmitLeaveCommand = new AsyncRelayCommand(SubmitLeave);
            ApproveLeaveCommand = new AsyncRelayCommand<LeaveRequest>(ApproveLeave);
            RejectLeaveCommand = new AsyncRelayCommand<LeaveRequest>(RejectLeave);
            LoadLeaveRequestsCommand = new AsyncRelayCommand(LoadLeaveRequests);
            ResetFormCommand = new RelayCommand(ResetForm);

            _ = LoadLeaveRequests(); // Load on startup
            ResetForm(); // Initialize with current dates
        }

        private async Task SubmitLeave()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewLeaveRequest.Reason) || NewLeaveRequest.StartDate == default || NewLeaveRequest.EndDate == default)
            {
                ErrorMessage = "Please fill in all required fields.";
                IsLoading = false;
                return;
            }

            if (NewLeaveRequest.EndDate < NewLeaveRequest.StartDate)
            {
                ErrorMessage = "End Date cannot be before Start Date.";
                IsLoading = false;
                return;
            }

            Employee? currentUser = null;
            if (_currentUserId == "master_admin")
            {
                currentUser = new Employee
                {
                    Id = _currentUserId,
                    Name = "Samiullah",
                    Username = "samiullah",
                    UserRole = _currentUserRole,
                    Position = "Master Admin",
                    Contact = "N/A",
                    Password = "N/A"
                };
            }
            else
            {
                currentUser = await _employeeService.GetEmployeeByIdAsync(_currentUserId, _currentUserRole);
            }

            if (currentUser == null)
            {
                ErrorMessage = "Current user information not found.";
                IsLoading = false;
                return;
            }

            NewLeaveRequest.EmployeeId = _currentUserId;
            NewLeaveRequest.EmployeeName = currentUser.Name;
            NewLeaveRequest.Status = LeaveStatus.Pending;
            var localNow = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            NewLeaveRequest.RequestedDate = TimeZoneInfo.ConvertTime(localNow, TimeZoneInfo.Local, _nzTimeZone);

            if (await _leaveService.SubmitLeaveRequestAsync(NewLeaveRequest))
            {
                SuccessMessage = "Leave request submitted successfully!";
                await _auditLogService.LogActionAsync(new AuditLog
                {
                    UserId = _currentUserId,
                    UserName = currentUser.Username,
                    Action = "SubmitLeaveRequest",
                    EntityType = "LeaveRequest",
                    EntityId = NewLeaveRequest.Id,
                    Details = $"Submitted leave request for {NewLeaveRequest.StartDate.ToShortDateString()} to {NewLeaveRequest.EndDate.ToShortDateString()} (Reason: {NewLeaveRequest.Reason}).",
                    IpAddress = "127.0.0.1", // Placeholder
                    Timestamp = DateTime.UtcNow
                });
                ResetForm();
                await LoadLeaveRequests();
            }
            else
            {
                ErrorMessage = "Failed to submit leave request.";
            }

            IsLoading = false;
        }

        private async Task ApproveLeave(LeaveRequest? request)
        {
            if (request == null || !CanManageLeaves)
            {
                ErrorMessage = "Invalid request or insufficient permissions.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            Employee? currentUser = null;
            if (_currentUserId == "master_admin")
            {
                currentUser = new Employee
                {
                    Id = _currentUserId,
                    Name = "Samiullah",
                    Username = "samiullah",
                    UserRole = _currentUserRole,
                    Position = "Master Admin",
                    Contact = "N/A",
                    Password = "N/A"
                };
            }
            else
            {
                currentUser = await _employeeService.GetEmployeeByIdAsync(_currentUserId, _currentUserRole);
            }

            if (currentUser == null)
            {
                ErrorMessage = "Current user information not found.";
                IsLoading = false;
                return;
            }

            // Prompt for manager comments
            string managerComments = "Approved."; // Default comment
            // You might want to implement a dialog for manager comments here

            if (await _leaveService.UpdateLeaveRequestStatusAsync(request.Id, LeaveStatus.Approved, managerComments))
            {
                SuccessMessage = $"Leave request for {request.EmployeeName} approved.";
                await _auditLogService.LogActionAsync(new AuditLog
                {
                    UserId = _currentUserId,
                    UserName = currentUser.Username,
                    Action = "ApproveLeaveRequest",
                    EntityType = "LeaveRequest",
                    EntityId = request.Id,
                    Details = $"Approved leave request for {request.EmployeeName} from {request.StartDate.ToShortDateString()} to {request.EndDate.ToShortDateString()}.",
                    IpAddress = "127.0.0.1", // Placeholder
                    Timestamp = DateTime.UtcNow
                });
                await LoadLeaveRequests();
            }
            else
            {
                ErrorMessage = "Failed to approve leave request.";
            }

            IsLoading = false;
        }

        private async Task RejectLeave(LeaveRequest? request)
        {
            if (request == null || !CanManageLeaves)
            {
                ErrorMessage = "Invalid request or insufficient permissions.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            Employee? currentUser = null;
            if (_currentUserId == "master_admin")
            {
                currentUser = new Employee
                {
                    Id = _currentUserId,
                    Name = "Samiullah",
                    Username = "samiullah",
                    UserRole = _currentUserRole,
                    Position = "Master Admin",
                    Contact = "N/A",
                    Password = "N/A"
                };
            }
            else
            {
                currentUser = await _employeeService.GetEmployeeByIdAsync(_currentUserId, _currentUserRole);
            }

            if (currentUser == null)
            {
                ErrorMessage = "Current user information not found.";
                IsLoading = false;
                return;
            }

            // Prompt for manager comments
            string managerComments = "Rejected."; // Default comment
            // You might want to implement a dialog for manager comments here

            if (await _leaveService.UpdateLeaveRequestStatusAsync(request.Id, LeaveStatus.Rejected, managerComments))
            {
                SuccessMessage = $"Leave request for {request.EmployeeName} rejected.";
                await _auditLogService.LogActionAsync(new AuditLog
                {
                    UserId = _currentUserId,
                    UserName = currentUser.Username,
                    Action = "RejectLeaveRequest",
                    EntityType = "LeaveRequest",
                    EntityId = request.Id,
                    Details = $"Rejected leave request for {request.EmployeeName} from {request.StartDate.ToShortDateString()} to {request.EndDate.ToShortDateString()}.",
                    IpAddress = "127.0.0.1", // Placeholder
                    Timestamp = DateTime.UtcNow
                });
                await LoadLeaveRequests();
            }
            else
            {
                ErrorMessage = "Failed to reject leave request.";
            }

            IsLoading = false;
        }

        private async Task LoadLeaveRequests()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            LeaveRequests.Clear();

            try
            {
                if (CanManageLeaves)
                {
                    // Managers/Admins/HR can see all pending/approved/rejected requests
                    var requests = await _leaveService.GetLeaveRequestsAsync();
                    foreach (var request in requests.OrderByDescending(r => r.RequestedDate))
                    {
                        LeaveRequests.Add(request);
                    }
                }
                else
                {
                    // Employees can only see their own requests
                    var requests = await _leaveService.GetEmployeeLeaveRequestsAsync(_currentUserId);
                    foreach (var request in requests.OrderByDescending(r => r.RequestedDate))
                    {
                        LeaveRequests.Add(request);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading leave requests: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ResetForm()
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _nzTimeZone);
            NewLeaveRequest = new LeaveRequest
            {
                StartDate = now.Date,
                EndDate = now.Date.AddDays(1),
                Status = LeaveStatus.Pending
            };
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            OnPropertyChanged(nameof(NewLeaveRequest));
        }
    }
} 