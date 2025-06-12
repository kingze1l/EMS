using EMS.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace EMS.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly IMongoCollection<Employee> _employees;
        private readonly IMongoCollection<PayrollRecord> _payrollRecords;

        public PayrollService(MongoDbContext dbContext)
        {
            _employees = dbContext.Database.GetCollection<Employee>("employees");
            _payrollRecords = dbContext.Database.GetCollection<PayrollRecord>("payrollRecords");
        }

        public async Task<bool> UpdateEmployeePayDetailsAsync(string employeeId, decimal basePay, decimal bonus, decimal deductions)
        {
            var filter = Builders<Employee>.Filter.Eq(e => e.Id, employeeId);
            var update = Builders<Employee>.Update
                .Set(e => e.BasePay, basePay)
                .Set(e => e.Bonus, bonus)
                .Set(e => e.Deductions, deductions);

            var result = await _employees.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<PayrollRecord?> GetPayrollRecordByIdAsync(string recordId)
        {
            return await _payrollRecords.Find(p => p.Id == recordId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PayrollRecord>> GetPayrollRecordsByEmployeeIdAsync(string employeeId)
        {
            return await _payrollRecords.Find(p => p.EmployeeId == employeeId)
                .SortByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PayrollRecord>> GetAllPayrollRecordsAsync()
        {
            return await _payrollRecords.Find(_ => true)
                .SortByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> CalculateNetPay(string employeeId)
        {
            var employee = await _employees.Find(e => e.Id == employeeId).FirstOrDefaultAsync();
            if (employee == null)
            {
                throw new ArgumentException("Employee not found.");
            }
            return employee.BasePay + employee.Bonus - employee.Deductions;
        }

        public async Task<bool> GeneratePayrollRecordAsync(string employeeId, string auditLogId)
        {
            var employee = await _employees.Find(e => e.Id == employeeId).FirstOrDefaultAsync();
            if (employee == null)
            {
                return false;
            }

            var netPay = await CalculateNetPay(employeeId);

            var payrollRecord = new PayrollRecord
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.Name,
                PaymentDate = DateTime.UtcNow,
                BasePay = employee.BasePay,
                Bonus = employee.Bonus,
                Deductions = employee.Deductions,
                NetPay = netPay,
                AuditLogId = auditLogId
            };

            await _payrollRecords.InsertOneAsync(payrollRecord);
            return true;
        }

        public async Task<IEnumerable<PayrollRecord>> GetPayrollRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var filter = Builders<PayrollRecord>.Filter.And(
                Builders<PayrollRecord>.Filter.Gte(p => p.PaymentDate, startDate),
                Builders<PayrollRecord>.Filter.Lte(p => p.PaymentDate, endDate)
            );
            
            return await _payrollRecords.Find(filter)
                .SortByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPayrollForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var records = await GetPayrollRecordsByDateRangeAsync(startDate, endDate);
            return records.Sum(r => r.NetPay);
        }

        public async Task<IEnumerable<PayrollRecord>> GetPayrollRecordsByEmployeeNameAsync(string employeeName)
        {
            var filter = Builders<PayrollRecord>.Filter.Regex(p => p.EmployeeName, 
                new MongoDB.Bson.BsonRegularExpression(employeeName, "i"));
            
            return await _payrollRecords.Find(filter)
                .SortByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<bool> BulkGeneratePayrollAsync(List<string> employeeIds, string auditLogId)
        {
            var payrollRecords = new List<PayrollRecord>();
            
            foreach (var employeeId in employeeIds)
            {
                var employee = await _employees.Find(e => e.Id == employeeId).FirstOrDefaultAsync();
                if (employee != null)
                {
                    var netPay = await CalculateNetPay(employeeId);
                    
                    var payrollRecord = new PayrollRecord
                    {
                        EmployeeId = employee.Id,
                        EmployeeName = employee.Name,
                        PaymentDate = DateTime.UtcNow,
                        BasePay = employee.BasePay,
                        Bonus = employee.Bonus,
                        Deductions = employee.Deductions,
                        NetPay = netPay,
                        AuditLogId = auditLogId
                    };
                    
                    payrollRecords.Add(payrollRecord);
                }
            }

            if (payrollRecords.Any())
            {
                await _payrollRecords.InsertManyAsync(payrollRecords);
                return true;
            }
            
            return false;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(string employeeId)
        {
            return await _employees.Find(e => e.Id == employeeId).FirstOrDefaultAsync();
        }
    }
} 