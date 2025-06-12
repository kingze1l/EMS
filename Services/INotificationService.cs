using EMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMS.Services
{
    public interface INotificationService
    {
        Task<List<Notification>> GetUserNotificationsAsync(string userId);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task MarkAsReadAsync(string notificationId);
        Task DeleteNotificationAsync(string notificationId);
        Task<int> GetUnreadCountAsync(string userId);
    }
} 