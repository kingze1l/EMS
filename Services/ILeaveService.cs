using EMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMS.Services
{
    public interface ILeaveService
    {
        Task<bool> SubmitLeaveRequestAsync(LeaveRequest leaveRequest);
        Task<List<LeaveRequest>> GetLeaveRequestsAsync();
        Task<List<LeaveRequest>> GetEmployeeLeaveRequestsAsync(string employeeId);
        Task<bool> UpdateLeaveRequestStatusAsync(string requestId, LeaveStatus newStatus, string managerComments);
        Task<bool> DeleteLeaveRequestAsync(string requestId);
    }
} 