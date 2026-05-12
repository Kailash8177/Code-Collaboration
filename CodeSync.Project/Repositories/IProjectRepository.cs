using CodeSync.Project.Models;

namespace CodeSync.Project.Repositories
{
    public interface IProjectRepository
    {
        // ── Project CRUD ──────────────────────────────────────────────────────
        Task<Models.Project?> FindByProjectIdAsync(int projectId);
        Task<IEnumerable<Models.Project>> FindByOwnerIdAsync(int ownerId);
        Task<IEnumerable<Models.Project>> FindAllAsync();
        Task<IEnumerable<Models.Project>> FindByVisibilityAsync(string visibility);
        Task<IEnumerable<Models.Project>> FindByLanguageAsync(string language);
        Task<IEnumerable<Models.Project>> SearchByNameAsync(string query);
        Task<IEnumerable<Models.Project>> FindByMemberUserIdAsync(int userId);
        Task<IEnumerable<Models.Project>> FindByIsArchivedAsync(bool isArchived);
        Task<int> CountByOwnerIdAsync(int ownerId);
        Task<Models.Project> CreateAsync(Models.Project project);
        Task<Models.Project> UpdateAsync(Models.Project project);
        Task DeleteAsync(int projectId);

        // ── Members ───────────────────────────────────────────────────────────
        Task<ProjectMember?> FindMemberAsync(int projectId, int userId);
        Task<IEnumerable<ProjectMember>> FindMembersByProjectIdAsync(int projectId);
        Task<ProjectMember> AddMemberAsync(ProjectMember member);
        Task RemoveMemberAsync(int projectId, int userId);

        // ── Stars ─────────────────────────────────────────────────────────────
        Task<ProjectStar?> FindStarAsync(int projectId, int userId);
        Task<ProjectStar> AddStarAsync(ProjectStar star);
        Task RemoveStarAsync(int projectId, int userId);
    }
}