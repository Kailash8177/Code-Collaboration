using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSync.File.Models
{
    public class CodeFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FileId { get; set; }

        [Required(ErrorMessage = "ProjectId is required")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "File name is required")]
        [StringLength(255, MinimumLength = 1,
            ErrorMessage = "File name must be between 1 and 255 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "File path is required")]
        [StringLength(1000, ErrorMessage = "File path too long")]
        public string Path { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Language name too long")]
        public string Language { get; set; } = string.Empty;

        // Full file content stored as text
        public string Content { get; set; } = string.Empty;

        [Range(0, long.MaxValue)]
        public long Size { get; set; } = 0;

        [Required]
        public int CreatedById { get; set; }

        public int LastEditedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft delete — file can be restored
        public bool IsDeleted { get; set; } = false;

        // Is this a folder or a file
        public bool IsFolder { get; set; } = false;

        public override string ToString() =>
            $"CodeFile[{FileId}] {Name} Path={Path} Project={ProjectId}";
    }
}