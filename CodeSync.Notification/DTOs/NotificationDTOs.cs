using System.ComponentModel.DataAnnotations;

namespace CodeSync.Notification.DTOs
{
    public class SendNotificationRequest
    {
        [Required]
        public int RecipientId { get; set; }

        [Required]
        public int ActorId { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        public string RelatedId { get; set; } = string.Empty;

        public string RelatedType { get; set; } = string.Empty;

        public bool SendEmail { get; set; } = false;
    }

    public class SendBulkNotificationRequest
    {
        [Required]
        public List<int> RecipientIds { get; set; } = new();

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
    }

    public class NotificationResponse
    {
        public int NotificationId { get; set; }
        public int RecipientId { get; set; }
        public int ActorId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string RelatedId { get; set; } = string.Empty;
        public string RelatedType { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UnreadCountResponse
    {
        public int RecipientId { get; set; }
        public int UnreadCount { get; set; }
    }
}