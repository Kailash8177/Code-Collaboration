using CodeSync.Execution.Models;

namespace CodeSync.Execution.Repositories
{
    public interface IExecutionRepository
    {
        // ── Jobs ──────────────────────────────────────────────────────────────
        Task<ExecutionJob?> FindByJobIdAsync(Guid jobId);
        Task<IEnumerable<ExecutionJob>> FindByUserIdAsync(int userId);
        Task<IEnumerable<ExecutionJob>> FindByProjectIdAsync(int projectId);
        Task<IEnumerable<ExecutionJob>> FindByStatusAsync(string status);
        Task<IEnumerable<ExecutionJob>> FindByLanguageAsync(string language);
        Task<int> CountByUserIdAsync(int userId);
        Task<ExecutionJob> CreateAsync(ExecutionJob job);
        Task<ExecutionJob> UpdateAsync(ExecutionJob job);

        // ── Languages ─────────────────────────────────────────────────────────
        Task<IEnumerable<SupportedLanguage>> GetSupportedLanguagesAsync();
        Task<SupportedLanguage?> FindLanguageByNameAsync(string name);
    }
}