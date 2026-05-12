using System.ComponentModel.DataAnnotations;

namespace CodeSync.Collab.DTOs
{
    // ── Create Session ────────────────────────────────────────────────────────

    public class CreateSessionRequest
    {
        [Required(ErrorMessage = "ProjectId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid ProjectId")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "FileId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid FileId")]
        public int FileId { get; set; }

        [StringLength(50)]
        public string Language { get; set; } = string.Empty;

        [Range(1, 50,
            ErrorMessage = "Max participants must be between 1 and 50")]
        public int MaxParticipants { get; set; } = 10;

        public bool IsPasswordProtected { get; set; } = false;

        [StringLength(100, ErrorMessage = "Password too long")]
        public string? SessionPassword { get; set; }
    }

    // ── Join Session ──────────────────────────────────────────────────────────

    public class JoinSessionRequest
    {
        [Required(ErrorMessage = "SessionId is required")]
        public Guid SessionId { get; set; }

        [StringLength(100)]
        public string? Password { get; set; }
    }

    // ── Update Cursor ─────────────────────────────────────────────────────────

    public class UpdateCursorRequest
    {
        [Required]
        public Guid SessionId { get; set; }

        [Range(0, int.MaxValue)]
        public int Line { get; set; }

        [Range(0, int.MaxValue)]
        public int Col { get; set; }
    }

    // ── Responses ─────────────────────────────────────────────────────────────

    public class SessionResponse
    {
        public Guid SessionId { get; set; }
        public int ProjectId { get; set; }
        public int FileId { get; set; }
        public int OwnerId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public bool IsPasswordProtected { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int ParticipantCount { get; set; }
    }

    public class ParticipantResponse
    {
        public int ParticipantId { get; set; }
        public Guid SessionId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public int CursorLine { get; set; }
        public int CursorCol { get; set; }
        public string Color { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    // ── SignalR Payloads ──────────────────────────────────────────────────────

    public class CursorUpdatePayload
    {
        public int UserId { get; set; }
        public string Color { get; set; } = string.Empty;
        public int Line { get; set; }
        public int Col { get; set; }
    }

    public class EditorChangePayload
    {
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ParticipantJoinedPayload
    {
        public int UserId { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}