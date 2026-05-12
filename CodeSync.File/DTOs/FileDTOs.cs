using System.ComponentModel.DataAnnotations;

namespace CodeSync.File.DTOs
{
    // ── Create File ───────────────────────────────────────────────────────────

    public class CreateFileRequest
    {
        [Required(ErrorMessage = "ProjectId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid ProjectId")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "File name is required")]
        [StringLength(255, MinimumLength = 1,
            ErrorMessage = "File name must be between 1 and 255 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "File path is required")]
        [StringLength(1000, ErrorMessage = "Path too long")]
        public string Path { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Language name too long")]
        public string Language { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public bool IsFolder { get; set; } = false;
    }

    // ── Create Folder ─────────────────────────────────────────────────────────

    public class CreateFolderRequest
    {
        [Required(ErrorMessage = "ProjectId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid ProjectId")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Folder name is required")]
        [StringLength(255, MinimumLength = 1,
            ErrorMessage = "Folder name must be between 1 and 255 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Folder path is required")]
        [StringLength(1000, ErrorMessage = "Path too long")]
        public string Path { get; set; } = string.Empty;
    }

    // ── Update File Content ───────────────────────────────────────────────────

    public class UpdateFileContentRequest
    {
        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = string.Empty;
    }

    // ── Rename File ───────────────────────────────────────────────────────────

    public class RenameFileRequest
    {
        [Required(ErrorMessage = "New name is required")]
        [StringLength(255, MinimumLength = 1,
            ErrorMessage = "File name must be between 1 and 255 characters")]
        public string NewName { get; set; } = string.Empty;
    }

    // ── Move File ─────────────────────────────────────────────────────────────

    public class MoveFileRequest
    {
        [Required(ErrorMessage = "New path is required")]
        [StringLength(1000, ErrorMessage = "Path too long")]
        public string NewPath { get; set; } = string.Empty;
    }

    // ── Responses ─────────────────────────────────────────────────────────────

    public class FileResponse
    {
        public int FileId { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public long Size { get; set; }
        public int CreatedById { get; set; }
        public int LastEditedBy { get; set; }
        public bool IsFolder { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class FileContentResponse
    {
        public int FileId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class FileTreeNode
    {
        public int FileId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IsFolder { get; set; }
        public string Language { get; set; } = string.Empty;
        public List<FileTreeNode> Children { get; set; } = new();
    }

    public class SearchResultResponse
    {
        public int FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int LineNumber { get; set; }
        public string LineContent { get; set; } = string.Empty;
    }
}