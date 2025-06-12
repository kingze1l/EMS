using EMS.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMS.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IMongoCollection<Notification> _notifications;

        public NotificationService(IMongoDatabase database)
        {
            _notifications = database.GetCollection<Notification>("notifications");
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _notifications
                .Find(n => n.UserId == userId)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;
            await _notifications.InsertOneAsync(notification);
            return notification;
        }

        public async Task MarkAsReadAsync(string notificationId)
        {
            await _notifications.UpdateOneAsync(
                n => n.Id == notificationId,
                Builders<Notification>.Update.Set(n => n.IsRead, true)
            );
        }

        public async Task DeleteNotificationAsync(string notificationId)
        {
            await _notifications.DeleteOneAsync(n => n.Id == notificationId);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return (int)await _notifications.CountDocumentsAsync(
                n => n.UserId == userId && !n.IsRead
            );
        }
    }
} 