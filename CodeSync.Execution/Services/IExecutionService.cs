using CodeSync.Execution.DTOs;

namespace CodeSync.Execution.Services
{
    public interface IExecutionService
    {
        Task<ExecutionJobResponse> SubmitExecutionAsync(
            int userId, SubmitExecutionRequest request);
        Task<ExecutionJobResponse> GetJobByIdAsync(Guid jobId);
        Task<IEnumerable<ExecutionJobResponse>> GetExecutionsByUserAsync(
            int userId);
        Task<IEnumerable<ExecutionJobResponse>> GetExecutionsByProjectAsync(
            int projectId);
        Task CancelExecutionAsync(Guid jobId, int userId);
        Task<ExecutionJobResponse> GetExecutionResultAsync(Guid jobId);
        Task<IEnumerable<SupportedLanguageResponse>> GetSupportedLanguagesAsync();
        Task<ExecutionStatsResponse> GetExecutionStatsAsync(int userId);
    }
}