using EMS.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMS.Services
{
    public interface IAuditLogService
    {
        Task<AuditLog> LogActionAsync(AuditLog log);
        Task<List<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<AuditLog>> GetLogsByUserAsync(string userId);
        Task<List<AuditLog>> GetLogsByEntityAsync(string entityType, string entityId);
        Task<List<AuditLog>> GetRecentLogsAsync(int count = 100);
    }
} 