using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSync.Execution.Models
{
    public class ExecutionJob
    {
        [Key]
        public Guid JobId { get; set; } = Guid.NewGuid();

        [Required]
        public int ProjectId { get; set; }

        public int FileId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Language name too long")]
        public string Language { get; set; } = string.Empty;

        [Required]
        public string SourceCode { get; set; } = string.Empty;

        public string Stdin { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            "QUEUED|RUNNING|COMPLETED|FAILED|TIMED_OUT|CANCELLED",
            ErrorMessage = "Invalid status")]
        public string Status { get; set; } = "QUEUED";

        public string Stdout { get; set; } = string.Empty;

        public string Stderr { get; set; } = string.Empty;

        public int ExitCode { get; set; } = 0;

        public long ExecutionTimeMs { get; set; } = 0;

        public long MemoryUsedKb { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }
    }

    public class SupportedLanguage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Version { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FileExtension { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string RunCommand { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;
    }
}