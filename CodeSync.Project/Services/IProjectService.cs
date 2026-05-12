using CodeSync.Project.DTOs;

namespace CodeSync.Project.Services
{
    public interface IProjectService
    {
        // ── Project CRUD ──────────────────────────────────────────────────────
        Task<ProjectResponse> CreateProjectAsync(
            int ownerId, CreateProjectRequest request);
        Task<ProjectResponse> GetProjectByIdAsync(
            int projectId, int? requestingUserId);
        Task<IEnumerable<ProjectResponse>> GetProjectsByOwnerAsync(int ownerId);
        Task<IEnumerable<ProjectResponse>> GetPublicProjectsAsync();
        Task<IEnumerable<ProjectResponse>> GetAllProjectsAdminAsync();
        Task<IEnumerable<ProjectResponse>> SearchProjectsAsync(string query);
        Task<IEnumerable<ProjectResponse>> GetProjectsByMemberAsync(int userId);
        Task<IEnumerable<ProjectResponse>> GetProjectsByLanguageAsync(string language);
        Task<ProjectResponse> UpdateProjectAsync(
            int projectId, int requestingUserId, UpdateProjectRequest request);
        Task ArchiveProjectAsync(int projectId, int requestingUserId);
        Task DeleteProjectAsync(int projectId, int requestingUserId);
        Task DeleteProjectAdminAsync(int projectId);

        // ── Fork and Star ─────────────────────────────────────────────────────
        Task<ProjectResponse> ForkProjectAsync(int projectId, int requestingUserId);
        Task StarProjectAsync(int projectId, int userId);
        Task UnstarProjectAsync(int projectId, int userId);

        // ── Members ───────────────────────────────────────────────────────────
        Task<IEnumerable<ProjectMemberResponse>> GetMembersAsync(int projectId);
        Task AddMemberAsync(
            int projectId, int requestingUserId, AddMemberRequest request);
        Task RemoveMemberAsync(
            int projectId, int requestingUserId, int targetUserId);
    }
}