using MyPhotoBiz.Models;

namespace MyPhotoBiz.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int limit = 10);
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<Notification?> GetNotificationByIdAsync(int id);
        Task CreateNotificationAsync(Notification notification);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);
        Task DeleteNotificationAsync(int id);
        Task DeleteOldNotificationsAsync(int daysOld = 30);
    }
}
