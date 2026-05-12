using CodeSync.Execution.Data;
using CodeSync.Execution.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.Execution.Repositories
{
    public class ExecutionRepository : IExecutionRepository
    {
        private readonly ExecutionDbContext _db;

        public ExecutionRepository(ExecutionDbContext db)
        {
            _db = db;
        }

        public async Task<ExecutionJob?> FindByJobIdAsync(Guid jobId) =>
            await _db.ExecutionJobs.FindAsync(jobId);

        public async Task<IEnumerable<ExecutionJob>> FindByUserIdAsync(
            int userId) =>
            await _db.ExecutionJobs
                .Where(j => j.UserId == userId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<ExecutionJob>> FindByProjectIdAsync(
            int projectId) =>
            await _db.ExecutionJobs
                .Where(j => j.ProjectId == projectId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<ExecutionJob>> FindByStatusAsync(
            string status) =>
            await _db.ExecutionJobs
                .Where(j => j.Status == status)
                .ToListAsync();

        public async Task<IEnumerable<ExecutionJob>> FindByLanguageAsync(
            string language) =>
            await _db.ExecutionJobs
                .Where(j => j.Language == language)
                .ToListAsync();

        public async Task<int> CountByUserIdAsync(int userId) =>
            await _db.ExecutionJobs
                .CountAsync(j => j.UserId == userId);

        public async Task<ExecutionJob> CreateAsync(ExecutionJob job)
        {
            _db.ExecutionJobs.Add(job);
            await _db.SaveChangesAsync();
            return job;
        }

        public async Task<ExecutionJob> UpdateAsync(ExecutionJob job)
        {
            _db.ExecutionJobs.Update(job);
            await _db.SaveChangesAsync();
            return job;
        }

        public async Task<IEnumerable<SupportedLanguage>>
            GetSupportedLanguagesAsync() =>
            await _db.SupportedLanguages
                .Where(l => l.IsEnabled)
                .ToListAsync();

        public async Task<SupportedLanguage?> FindLanguageByNameAsync(
            string name) =>
            await _db.SupportedLanguages
                .FirstOrDefaultAsync(l =>
                    l.Name == name && l.IsEnabled);
    }
}