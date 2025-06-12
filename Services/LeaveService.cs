using EMS.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace EMS.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly IMongoCollection<LeaveRequest> _leaveRequests;

        public LeaveService(IMongoDatabase database)
        {
            _leaveRequests = database.GetCollection<LeaveRequest>("leaveRequests");
        }

        public async Task<bool> SubmitLeaveRequestAsync(LeaveRequest leaveRequest)
        {
            if (leaveRequest.EndDate < leaveRequest.StartDate)
            {
                // Invalid date range
                return false;
            }

            try
            {
                await _leaveRequests.InsertOneAsync(leaveRequest);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting leave request: {ex.Message}");
                return false;
            }
        }

        public async Task<List<LeaveRequest>> GetLeaveRequestsAsync()
        {
            try
            {
                return await _leaveRequests.Find(request => true).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting leave requests: {ex.Message}");
                return new List<LeaveRequest>();
            }
        }

        public async Task<List<LeaveRequest>> GetEmployeeLeaveRequestsAsync(string employeeId)
        {
            try
            {
                return await _leaveRequests.Find(request => request.EmployeeId == employeeId).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employee leave requests: {ex.Message}");
                return new List<LeaveRequest>();
            }
        }

        public async Task<bool> UpdateLeaveRequestStatusAsync(string requestId, LeaveStatus newStatus, string managerComments)
        {
            try
            {
                var filter = Builders<LeaveRequest>.Filter.Eq(request => request.Id, requestId);
                var update = Builders<LeaveRequest>.Update
                    .Set(request => request.Status, newStatus)
                    .Set(request => request.ManagerComments, managerComments);

                var result = await _leaveRequests.UpdateOneAsync(filter, update);
                return result.IsAcknowledged && result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating leave request status: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteLeaveRequestAsync(string requestId)
        {
            try
            {
                var filter = Builders<LeaveRequest>.Filter.Eq(request => request.Id, requestId);
                var result = await _leaveRequests.DeleteOneAsync(filter);
                return result.IsAcknowledged && result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting leave request: {ex.Message}");
                return false;
            }
        }
    }
} 