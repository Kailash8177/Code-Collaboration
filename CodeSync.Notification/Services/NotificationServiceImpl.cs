using CodeSync.Notification.DTOs;
using CodeSync.Notification.Repositories;
using Microsoft.AspNetCore.SignalR;
using CodeSync.Notification.Hubs;

namespace CodeSync.Notification.Services
{
    public class NotificationServiceImpl : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationServiceImpl> _logger;

        public NotificationServiceImpl(
            INotificationRepository repo,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationServiceImpl> logger)
        {
            _repo       = repo;
            _hubContext = hubContext;
            _logger     = logger;
        }

        // ── Send Single Notification ──────────────────────────────────────────

        public async Task<NotificationResponse> SendAsync(
            SendNotificationRequest req)
        {
            var notification = new Models.Notification
            {
                RecipientId = req.RecipientId,
                ActorId     = req.ActorId,
                Type        = req.Type,
                Title       = req.Title,
                Message     = req.Message,
                RelatedId   = req.RelatedId,
                RelatedType = req.RelatedType,
                IsRead      = false
            };

            await _repo.CreateAsync(notification);

            // ✅ Push real-time badge count via SignalR
            var unreadCount = await _repo
                .CountUnreadByRecipientIdAsync(req.RecipientId);

            await _hubContext.Clients
                .Group($"user-{req.RecipientId}")
                .SendAsync("ReceiveNotification", new
                {
                    notification = MapToResponse(notification),
                    unreadCount
                });

            _logger.LogInformation(
                "Notification sent to RecipientId={RecipientId} Type={Type}",
                req.RecipientId, req.Type);

            return MapToResponse(notification);
        }

        // ── Send Bulk Notification ────────────────────────────────────────────

        public async Task SendBulkAsync(SendBulkNotificationRequest req)
        {
            foreach (var recipientId in req.RecipientIds)
            {
                var notification = new Models.Notification
                {
                    RecipientId = recipientId,
                    ActorId     = 0, // system
                    Type        = req.Type,
                    Title       = req.Title,
                    Message     = req.Message,
                    IsRead      = false
                };

                await _repo.CreateAsync(notification);

                // Push to each user
                await _hubContext.Clients
                    .Group($"user-{recipientId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        notification = MapToResponse(notification)
                    });
            }

            _logger.LogInformation(
                "Bulk notification sent to {Count} recipients",
                req.RecipientIds.Count);
        }

        // ── Get Notifications ─────────────────────────────────────────────────

        public async Task<IEnumerable<NotificationResponse>>
            GetByRecipientAsync(int recipientId)
        {
            var notifications = await _repo
                .FindByRecipientIdAsync(recipientId);
            return notifications.Select(MapToResponse);
        }

        public async Task<IEnumerable<NotificationResponse>>
            GetUnreadAsync(int recipientId)
        {
            var notifications = await _repo
                .FindUnreadByRecipientIdAsync(recipientId);
            return notifications.Select(MapToResponse);
        }

        public async Task<UnreadCountResponse> GetUnreadCountAsync(
            int recipientId)
        {
            var count = await _repo
                .CountUnreadByRecipientIdAsync(recipientId);
            return new UnreadCountResponse
            {
                RecipientId = recipientId,
                UnreadCount = count
            };
        }

        // ── Mark as Read ──────────────────────────────────────────────────────

        public async Task MarkAsReadAsync(int notificationId, int recipientId)
        {
            var notification = await _repo
                .FindByNotificationIdAsync(notificationId)
                ?? throw new KeyNotFoundException(
                    $"Notification {notificationId} not found.");

            if (notification.RecipientId != recipientId)
                throw new UnauthorizedAccessException(
                    "Cannot mark another user's notification as read.");

            notification.IsRead = true;
            await _repo.UpdateAsync(notification);

            // Update badge count via SignalR
            var unreadCount = await _repo
                .CountUnreadByRecipientIdAsync(recipientId);

            await _hubContext.Clients
                .Group($"user-{recipientId}")
                .SendAsync("UpdateUnreadCount", unreadCount);
        }

        public async Task MarkAllReadAsync(int recipientId)
        {
            await _repo.MarkAllReadAsync(recipientId);

            // Reset badge count to 0
            await _hubContext.Clients
                .Group($"user-{recipientId}")
                .SendAsync("UpdateUnreadCount", 0);
        }

        // ── Delete ────────────────────────────────────────────────────────────

        public async Task DeleteAsync(int notificationId, int recipientId)
        {
            var notification = await _repo
                .FindByNotificationIdAsync(notificationId)
                ?? throw new KeyNotFoundException(
                    $"Notification {notificationId} not found.");

            if (notification.RecipientId != recipientId)
                throw new UnauthorizedAccessException(
                    "Cannot delete another user's notification.");

            await _repo.DeleteAsync(notificationId);
        }

        public async Task DeleteAllReadAsync(int recipientId)
        {
            await _repo.DeleteAllReadAsync(recipientId);
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static NotificationResponse MapToResponse(
            Models.Notification n) => new()
        {
            NotificationId = n.NotificationId,
            RecipientId    = n.RecipientId,
            ActorId        = n.ActorId,
            Type           = n.Type,
            Title          = n.Title,
            Message        = n.Message,
            RelatedId      = n.RelatedId,
            RelatedType    = n.RelatedType,
            IsRead         = n.IsRead,
            CreatedAt      = n.CreatedAt
        };
    }
}