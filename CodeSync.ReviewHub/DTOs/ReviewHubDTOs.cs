using System.ComponentModel.DataAnnotations;

namespace CodeSync.ReviewHub.DTOs
{
    // ── Snapshot DTOs ─────────────────────────────────────────────────────────

    public class CreateSnapshotRequest
    {
        [Required(ErrorMessage = "FileId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid FileId")]
        public int FileId { get; set; }

        [Required(ErrorMessage = "ProjectId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid ProjectId")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Commit message is required")]
        [StringLength(500, MinimumLength = 1,
            ErrorMessage = "Message must be between 1 and 500 characters")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = string.Empty;

        [StringLength(100)]
        public string Branch { get; set; } = "main";
    }

    public class CreateBranchRequest
    {
        [Required(ErrorMessage = "Branch name is required")]
        [StringLength(100, MinimumLength = 1,
            ErrorMessage = "Branch name must be between 1 and 100 characters")]
        public string BranchName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int FileId { get; set; }
    }

    public class TagSnapshotRequest
    {
        [Required(ErrorMessage = "Tag is required")]
        [StringLength(50, MinimumLength = 1,
            ErrorMessage = "Tag must be between 1 and 50 characters")]
        public string Tag { get; set; } = string.Empty;
    }

    public class SnapshotResponse
    {
        public int SnapshotId { get; set; }
        public int ProjectId { get; set; }
        public int FileId { get; set; }
        public int AuthorId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string? Tag { get; set; }
        public int? ParentSnapshotId { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DiffResponse
    {
        public int SnapshotId1 { get; set; }
        public int SnapshotId2 { get; set; }
        public string DiffResult { get; set; } = string.Empty;
        public int LinesAdded { get; set; }
        public int LinesRemoved { get; set; }
        public int LinesUnchanged { get; set; }
    }

    // ── Comment DTOs ──────────────────────────────────────────────────────────

    public class AddCommentRequest
    {
        [Required(ErrorMessage = "ProjectId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid ProjectId")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "FileId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid FileId")]
        public int FileId { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [StringLength(2000, MinimumLength = 1,
            ErrorMessage = "Comment must be between 1 and 2000 characters")]
        public string Content { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Line number must be positive")]
        public int LineNumber { get; set; }

        public int ColumnNumber { get; set; } = 0;

        public int? ParentCommentId { get; set; }

        public int? SnapshotId { get; set; }
    }

    public class UpdateCommentRequest
    {
        [Required(ErrorMessage = "Content is required")]
        [StringLength(2000, MinimumLength = 1,
            ErrorMessage = "Comment must be between 1 and 2000 characters")]
        public string Content { get; set; } = string.Empty;
    }

    public class CommentResponse
    {
        public int CommentId { get; set; }
        public int ProjectId { get; set; }
        public int FileId { get; set; }
        public int AuthorId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public int? ParentCommentId { get; set; }
        public bool IsResolved { get; set; }
        public int? SnapshotId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}