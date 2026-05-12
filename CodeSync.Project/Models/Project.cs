using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSync.Project.Models
{
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Owner is required")]
        public int OwnerId { get; set; }

        [Required(ErrorMessage = "Project name is required")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Project name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Language is required")]
        [StringLength(50, ErrorMessage = "Language name too long")]
        public string Language { get; set; } = string.Empty;

        [Required]
        [RegularExpression("PUBLIC|PRIVATE",
            ErrorMessage = "Visibility must be PUBLIC or PRIVATE")]
        public string Visibility { get; set; } = "PUBLIC";

        public int? TemplateId { get; set; }

        public bool IsArchived { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Range(0, int.MaxValue,
            ErrorMessage = "Star count cannot be negative")]
        public int StarCount { get; set; } = 0;

        [Range(0, int.MaxValue,
            ErrorMessage = "Fork count cannot be negative")]
        public int ForkCount { get; set; } = 0;

        // Navigation
        public ICollection<ProjectMember> Members { get; set; }
            = new List<ProjectMember>();
        public ICollection<ProjectStar> Stars { get; set; }
            = new List<ProjectStar>();
    }

    public class ProjectMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [RegularExpression("OWNER|MEMBER|VIEWER",
            ErrorMessage = "Role must be OWNER, MEMBER or VIEWER")]
        public string Role { get; set; } = "MEMBER";

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProjectId")]
        public Project Project { get; set; } = null!;
    }

    public class ProjectStar
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime StarredAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProjectId")]
        public Project Project { get; set; } = null!;
    }
}