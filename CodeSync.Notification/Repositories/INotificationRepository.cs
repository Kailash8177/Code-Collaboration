using CodeSync.Notification.Models;

namespace CodeSync.Notification.Repositories
{
    public interface INotificationRepository
    {
        Task<Models.Notification?> FindByNotificationIdAsync(int notificationId);
        Task<IEnumerable<Models.Notification>> FindByRecipientIdAsync(
            int recipientId);
        Task<IEnumerable<Models.Notification>> FindUnreadByRecipientIdAsync(
            int recipientId);
        Task<int> CountUnreadByRecipientIdAsync(int recipientId);
        Task<IEnumerable<Models.Notification>> FindByTypeAsync(string type);
        Task<Models.Notification> CreateAsync(Models.Notification notification);
        Task<Models.Notification> UpdateAsync(Models.Notification notification);
        Task DeleteAsync(int notificationId);
        Task MarkAllReadAsync(int recipientId);
        Task DeleteAllReadAsync(int recipientId);
    }
}