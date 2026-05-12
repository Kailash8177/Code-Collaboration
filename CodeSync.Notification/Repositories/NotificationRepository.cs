using CodeSync.Notification.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.Notification.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _db;

        public NotificationRepository(NotificationDbContext db)
        {
            _db = db;
        }

        public async Task<Models.Notification?> FindByNotificationIdAsync(
            int notificationId) =>
            await _db.Notifications.FindAsync(notificationId);

        public async Task<IEnumerable<Models.Notification>>
            FindByRecipientIdAsync(int recipientId) =>
            await _db.Notifications
                .Where(n => n.RecipientId == recipientId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Models.Notification>>
            FindUnreadByRecipientIdAsync(int recipientId) =>
            await _db.Notifications
                .Where(n => n.RecipientId == recipientId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<int> CountUnreadByRecipientIdAsync(
            int recipientId) =>
            await _db.Notifications
                .CountAsync(n => n.RecipientId == recipientId && !n.IsRead);

        public async Task<IEnumerable<Models.Notification>> FindByTypeAsync(
            string type) =>
            await _db.Notifications
                .Where(n => n.Type == type)
                .ToListAsync();

        public async Task<Models.Notification> CreateAsync(
            Models.Notification notification)
        {
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
            return notification;
        }

        public async Task<Models.Notification> UpdateAsync(
            Models.Notification notification)
        {
            _db.Notifications.Update(notification);
            await _db.SaveChangesAsync();
            return notification;
        }

        public async Task DeleteAsync(int notificationId)
        {
            var notification = await _db.Notifications
                .FindAsync(notificationId);
            if (notification is not null)
            {
                _db.Notifications.Remove(notification);
                await _db.SaveChangesAsync();
            }
        }

        public async Task MarkAllReadAsync(int recipientId)
        {
            var unread = await _db.Notifications
                .Where(n => n.RecipientId == recipientId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAllReadAsync(int recipientId)
        {
            var read = await _db.Notifications
                .Where(n => n.RecipientId == recipientId && n.IsRead)
                .ToListAsync();

            _db.Notifications.RemoveRange(read);
            await _db.SaveChangesAsync();
        }
    }
}