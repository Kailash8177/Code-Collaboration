using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSync.Collab.Models
{
    public class CollabSession
    {
        [Key]
        public Guid SessionId { get; set; } = Guid.NewGuid();

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int FileId { get; set; }

        [Required]
        public int OwnerId { get; set; }

        [Required]
        [RegularExpression("ACTIVE|ENDED",
            ErrorMessage = "Status must be ACTIVE or ENDED")]
        public string Status { get; set; } = "ACTIVE";

        [StringLength(50)]
        public string Language { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EndedAt { get; set; }

        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

        [Range(1, 50,
            ErrorMessage = "Max participants must be between 1 and 50")]
        public int MaxParticipants { get; set; } = 10;

        public bool IsPasswordProtected { get; set; } = false;

        [StringLength(100)]
        public string? SessionPassword { get; set; }

        // Navigation
        public ICollection<Participant> Participants { get; set; }
            = new List<Participant>();
    }

    public class Participant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ParticipantId { get; set; }

        [Required]
        public Guid SessionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [RegularExpression("HOST|EDITOR|VIEWER",
            ErrorMessage = "Role must be HOST, EDITOR or VIEWER")]
        public string Role { get; set; } = "EDITOR";

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LeftAt { get; set; }

        public int CursorLine { get; set; } = 0;

        public int CursorCol { get; set; } = 0;

        [StringLength(20)]
        public string Color { get; set; } = "#FF5733";

        public bool IsActive { get; set; } = true;

        // Navigation
        [ForeignKey("SessionId")]
        public CollabSession Session { get; set; } = null!;
    }
}