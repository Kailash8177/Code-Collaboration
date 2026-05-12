using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSync.ReviewHub.Models
{
    public class Snapshot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SnapshotId { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int FileId { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1,
            ErrorMessage = "Commit message must be between 1 and 500 characters")]
        public string Message { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(64)]
        public string Hash { get; set; } = string.Empty;

        public int? ParentSnapshotId { get; set; }

        [StringLength(100)]
        public string Branch { get; set; } = "main";

        [StringLength(50)]
        public string? Tag { get; set; }

        public bool IsArchived { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommentId { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int FileId { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 1,
            ErrorMessage = "Comment must be between 1 and 2000 characters")]
        public string Content { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int LineNumber { get; set; }

        public int ColumnNumber { get; set; } = 0;

        public int? ParentCommentId { get; set; }

        public bool IsResolved { get; set; } = false;

        public int? SnapshotId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}