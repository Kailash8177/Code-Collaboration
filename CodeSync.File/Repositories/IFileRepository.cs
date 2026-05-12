using CodeSync.File.Models;

namespace CodeSync.File.Repositories
{
    public interface IFileRepository
    {
        Task<CodeFile?> FindByFileIdAsync(int fileId);
        Task<CodeFile?> FindByProjectIdAndPathAsync(int projectId, string path);
        Task<IEnumerable<CodeFile>> FindByProjectIdAsync(int projectId);
        Task<IEnumerable<CodeFile>> FindByLanguageAsync(string language);
        Task<IEnumerable<CodeFile>> FindByLastEditedByAsync(int userId);
        Task<IEnumerable<CodeFile>> FindByIsDeletedAsync(bool isDeleted);
        Task<int> CountByProjectIdAsync(int projectId);
        Task<CodeFile> CreateAsync(CodeFile file);
        Task<CodeFile> UpdateAsync(CodeFile file);
        Task DeleteByFileIdAsync(int fileId);
    }
}