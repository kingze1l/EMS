using EMS.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMS.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IMongoCollection<AuditLog> _auditLogs;

        public AuditLogService(IMongoDatabase database)
        {
            _auditLogs = database.GetCollection<AuditLog>("auditLogs");
        }

        public async Task<AuditLog> LogActionAsync(AuditLog log)
        {
            log.Timestamp = DateTime.UtcNow;
            await _auditLogs.InsertOneAsync(log);
            return log;
        }

        public async Task<List<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _auditLogs
                .Find(log => log.Timestamp >= startDate && log.Timestamp <= endDate)
                .SortByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetLogsByUserAsync(string userId)
        {
            return await _auditLogs
                .Find(log => log.UserId == userId)
                .SortByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetLogsByEntityAsync(string entityType, string entityId)
        {
            return await _auditLogs
                .Find(log => log.EntityType == entityType && log.EntityId == entityId)
                .SortByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetRecentLogsAsync(int count = 100)
        {
            return await _auditLogs
                .Find(_ => true)
                .SortByDescending(log => log.Timestamp)
                .Limit(count)
                .ToListAsync();
        }
    }
} 