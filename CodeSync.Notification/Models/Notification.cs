using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSync.Notification.Models
{
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        [Required]
        public int RecipientId { get; set; }

        [Required]
        public int ActorId { get; set; }

        [Required]
        [RegularExpression(
            "SESSION_INVITE|COMMENT|MENTION|SNAPSHOT|FORK|MEMBER_ADDED|EXECUTION|SYSTEM",
            ErrorMessage = "Invalid notification type")]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(200, ErrorMessage = "Title too long")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, ErrorMessage = "Message too long")]
        public string Message { get; set; } = string.Empty;

        [StringLength(100)]
        public string RelatedId { get; set; } = string.Empty;

        [StringLength(50)]
        public string RelatedType { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}