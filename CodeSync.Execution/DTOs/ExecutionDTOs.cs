using System.ComponentModel.DataAnnotations;

namespace CodeSync.Execution.DTOs
{
    // ── Submit Execution ──────────────────────────────────────────────────────

    public class SubmitExecutionRequest
    {
        [Required(ErrorMessage = "ProjectId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid ProjectId")]
        public int ProjectId { get; set; }

        public int FileId { get; set; }

        [Required(ErrorMessage = "Language is required")]
        [StringLength(50, ErrorMessage = "Language name too long")]
        public string Language { get; set; } = string.Empty;

        [Required(ErrorMessage = "Source code is required")]
        public string SourceCode { get; set; } = string.Empty;

        public string Stdin { get; set; } = string.Empty;
    }

    // ── Responses ─────────────────────────────────────────────────────────────

    public class ExecutionJobResponse
    {
        public Guid JobId { get; set; }
        public int ProjectId { get; set; }
        public int FileId { get; set; }
        public int UserId { get; set; }
        public string Language { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Stdout { get; set; } = string.Empty;
        public string Stderr { get; set; } = string.Empty;
        public int ExitCode { get; set; }
        public long ExecutionTimeMs { get; set; }
        public long MemoryUsedKb { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class SupportedLanguageResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
    }

    public class ExecutionStatsResponse
    {
        public int TotalJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int FailedJobs { get; set; }
        public int CancelledJobs { get; set; }
        public Dictionary<string, int> JobsByLanguage { get; set; } = new();
    }
}