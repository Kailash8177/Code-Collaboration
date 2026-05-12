using CodeSync.File.Data;
using CodeSync.File.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.File.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly FileDbContext _db;

        public FileRepository(FileDbContext db)
        {
            _db = db;
        }

        public async Task<CodeFile?> FindByFileIdAsync(int fileId) =>
            await _db.CodeFiles.FindAsync(fileId);

        public async Task<CodeFile?> FindByProjectIdAndPathAsync(
            int projectId, string path) =>
            await _db.CodeFiles
                .FirstOrDefaultAsync(f =>
                    f.ProjectId == projectId &&
                    f.Path == path &&
                    !f.IsDeleted);

        public async Task<IEnumerable<CodeFile>> FindByProjectIdAsync(
            int projectId) =>
            await _db.CodeFiles
                .Where(f => f.ProjectId == projectId && !f.IsDeleted)
                .OrderBy(f => f.Path)
                .ToListAsync();

        public async Task<IEnumerable<CodeFile>> FindByLanguageAsync(
            string language) =>
            await _db.CodeFiles
                .Where(f => f.Language == language && !f.IsDeleted)
                .ToListAsync();

        public async Task<IEnumerable<CodeFile>> FindByLastEditedByAsync(
            int userId) =>
            await _db.CodeFiles
                .Where(f => f.LastEditedBy == userId && !f.IsDeleted)
                .ToListAsync();

        public async Task<IEnumerable<CodeFile>> FindByIsDeletedAsync(
            bool isDeleted) =>
            await _db.CodeFiles
                .Where(f => f.IsDeleted == isDeleted)
                .ToListAsync();

        public async Task<int> CountByProjectIdAsync(int projectId) =>
            await _db.CodeFiles
                .CountAsync(f => f.ProjectId == projectId && !f.IsDeleted);

        public async Task<CodeFile> CreateAsync(CodeFile file)
        {
            _db.CodeFiles.Add(file);
            await _db.SaveChangesAsync();
            return file;
        }

        public async Task<CodeFile> UpdateAsync(CodeFile file)
        {
            file.UpdatedAt = DateTime.UtcNow;
            _db.CodeFiles.Update(file);
            await _db.SaveChangesAsync();
            return file;
        }

        public async Task DeleteByFileIdAsync(int fileId)
        {
            var file = await _db.CodeFiles.FindAsync(fileId);
            if (file is not null)
            {
                _db.CodeFiles.Remove(file);
                await _db.SaveChangesAsync();
            }
        }
    }
}