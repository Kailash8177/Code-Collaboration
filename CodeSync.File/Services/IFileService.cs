using CodeSync.File.DTOs;

namespace CodeSync.File.Services
{
    public interface IFileService
    {
        // ── File CRUD ─────────────────────────────────────────────────────────
        Task<FileResponse> CreateFileAsync(int userId, CreateFileRequest request);
        Task<FileResponse> GetFileByIdAsync(int fileId);
        Task<FileContentResponse> GetFileContentAsync(int fileId);
        Task<IEnumerable<FileResponse>> GetFilesByProjectAsync(int projectId);
        Task<FileContentResponse> UpdateFileContentAsync(
            int fileId, int userId, UpdateFileContentRequest request);
        Task<FileResponse> RenameFileAsync(
            int fileId, int userId, RenameFileRequest request);
        Task<FileResponse> MoveFileAsync(
            int fileId, int userId, MoveFileRequest request);
        Task DeleteFileAsync(int fileId, int userId);
        Task RestoreFileAsync(int fileId, int userId);

        // ── Folder ────────────────────────────────────────────────────────────
        Task<FileResponse> CreateFolderAsync(int userId, CreateFolderRequest request);

        // ── Tree + Search ─────────────────────────────────────────────────────
        Task<List<FileTreeNode>> GetFileTreeAsync(int projectId);
        Task<IEnumerable<SearchResultResponse>> SearchInProjectAsync(
            int projectId, string query);
    }
}