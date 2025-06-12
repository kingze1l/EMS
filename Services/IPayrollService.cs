using EMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMS.Services
{
    public interface IPayrollService
    {
        Task<bool> UpdateEmployeePayDetailsAsync(string employeeId, decimal basePay, decimal bonus, decimal deductions);
        Task<PayrollRecord?> GetPayrollRecordByIdAsync(string recordId);
        Task<IEnumerable<PayrollRecord>> GetPayrollRecordsByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<PayrollRecord>> GetAllPayrollRecordsAsync();
        Task<decimal> CalculateNetPay(string employeeId);
        Task<bool> GeneratePayrollRecordAsync(string employeeId, string auditLogId);
        Task<IEnumerable<PayrollRecord>> GetPayrollRecordsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalPayrollForPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<PayrollRecord>> GetPayrollRecordsByEmployeeNameAsync(string employeeName);
        Task<bool> BulkGeneratePayrollAsync(List<string> employeeIds, string auditLogId);
        Task<Employee?> GetEmployeeByIdAsync(string employeeId);
    }
} 