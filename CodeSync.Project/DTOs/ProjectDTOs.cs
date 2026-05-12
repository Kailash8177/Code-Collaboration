using System.ComponentModel.DataAnnotations;

namespace CodeSync.Project.DTOs
{
    // ── Create Project ────────────────────────────────────────────────────────

    public class CreateProjectRequest
    {
        [Required(ErrorMessage = "Project name is required")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Project name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9 _\-]+$",
            ErrorMessage = "Name can only contain letters, numbers, spaces, hyphens and underscores")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Language is required")]
        [StringLength(50, ErrorMessage = "Language name too long")]
        public string Language { get; set; } = string.Empty;

        [Required(ErrorMessage = "Visibility is required")]
        [RegularExpression("PUBLIC|PRIVATE",
            ErrorMessage = "Visibility must be PUBLIC or PRIVATE")]
        public string Visibility { get; set; } = "PUBLIC";
    }

    // ── Update Project ────────────────────────────────────────────────────────

    public class UpdateProjectRequest
    {
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Project name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9 _\-]+$",
            ErrorMessage = "Name can only contain letters, numbers, spaces, hyphens and underscores")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Language name too long")]
        public string? Language { get; set; }

        [RegularExpression("PUBLIC|PRIVATE",
            ErrorMessage = "Visibility must be PUBLIC or PRIVATE")]
        public string? Visibility { get; set; }
    }

    // ── Add Member ────────────────────────────────────────────────────────────

    public class AddMemberRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid UserId")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("MEMBER|VIEWER",
            ErrorMessage = "Role must be MEMBER or VIEWER")]
        public string Role { get; set; } = "MEMBER";
    }

    // ── Responses ─────────────────────────────────────────────────────────────

    public class ProjectResponse
    {
        public int ProjectId { get; set; }
        public int OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Visibility { get; set; } = string.Empty;
        public bool IsArchived { get; set; }
        public int StarCount { get; set; }
        public int ForkCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProjectMemberResponse
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}