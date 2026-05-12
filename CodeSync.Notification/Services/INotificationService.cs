using CodeSync.Notification.DTOs;

namespace CodeSync.Notification.Services
{
    public interface INotificationService
    {
        Task<NotificationResponse> SendAsync(SendNotificationRequest request);
        Task SendBulkAsync(SendBulkNotificationRequest request);
        Task<IEnumerable<NotificationResponse>> GetByRecipientAsync(
            int recipientId);
        Task<IEnumerable<NotificationResponse>> GetUnreadAsync(int recipientId);
        Task<UnreadCountResponse> GetUnreadCountAsync(int recipientId);
        Task MarkAsReadAsync(int notificationId, int recipientId);
        Task MarkAllReadAsync(int recipientId);
        Task DeleteAsync(int notificationId, int recipientId);
        Task DeleteAllReadAsync(int recipientId);
    }
}