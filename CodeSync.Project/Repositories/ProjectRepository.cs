using CodeSync.Project.Data;
using CodeSync.Project.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.Project.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ProjectDbContext _db;

        public ProjectRepository(ProjectDbContext db)
        {
            _db = db;
        }

        public async Task<Models.Project?> FindByProjectIdAsync(int projectId) =>
            await _db.Projects
                .Include(p => p.Members)
                .Include(p => p.Stars)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

        public async Task<IEnumerable<Models.Project>> FindByOwnerIdAsync(int ownerId) =>
            await _db.Projects
                .Where(p => p.OwnerId == ownerId)
                .OrderByDescending(p => p.UpdatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Models.Project>> FindAllAsync() =>
            await _db.Projects
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Models.Project>> FindByVisibilityAsync(
            string visibility) =>
            await _db.Projects
                .Where(p => p.Visibility == visibility && !p.IsArchived)
                .OrderByDescending(p => p.StarCount)
                .ToListAsync();

        public async Task<IEnumerable<Models.Project>> FindByLanguageAsync(
            string language) =>
            await _db.Projects
                .Where(p => p.Language == language
                    && p.Visibility == "PUBLIC"
                    && !p.IsArchived)
                .ToListAsync();

        public async Task<IEnumerable<Models.Project>> SearchByNameAsync(string query) =>
            await _db.Projects
                .Where(p => p.Name.Contains(query)
                    && p.Visibility == "PUBLIC"
                    && !p.IsArchived)
                .Take(20)
                .ToListAsync();

        public async Task<IEnumerable<Models.Project>> FindByMemberUserIdAsync(
            int userId) =>
            await _db.Projects
                .Where(p => p.Members.Any(m => m.UserId == userId))
                .ToListAsync();

        public async Task<IEnumerable<Models.Project>> FindByIsArchivedAsync(
            bool isArchived) =>
            await _db.Projects
                .Where(p => p.IsArchived == isArchived)
                .ToListAsync();

        public async Task<int> CountByOwnerIdAsync(int ownerId) =>
            await _db.Projects
                .CountAsync(p => p.OwnerId == ownerId);

        public async Task<Models.Project> CreateAsync(Models.Project project)
        {
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
            return project;
        }

        public async Task<Models.Project> UpdateAsync(Models.Project project)
        {
            project.UpdatedAt = DateTime.UtcNow;
            _db.Projects.Update(project);
            await _db.SaveChangesAsync();
            return project;
        }

        public async Task DeleteAsync(int projectId)
        {
            var project = await _db.Projects.FindAsync(projectId);
            if (project is not null)
            {
                _db.Projects.Remove(project);
                await _db.SaveChangesAsync();
            }
        }

        // ── Members ───────────────────────────────────────────────────────────

        public async Task<ProjectMember?> FindMemberAsync(int projectId, int userId) =>
            await _db.ProjectMembers
                .FirstOrDefaultAsync(m =>
                    m.ProjectId == projectId && m.UserId == userId);

        public async Task<IEnumerable<ProjectMember>> FindMembersByProjectIdAsync(
            int projectId) =>
            await _db.ProjectMembers
                .Where(m => m.ProjectId == projectId)
                .ToListAsync();

        public async Task<ProjectMember> AddMemberAsync(ProjectMember member)
        {
            _db.ProjectMembers.Add(member);
            await _db.SaveChangesAsync();
            return member;
        }

        public async Task RemoveMemberAsync(int projectId, int userId)
        {
            var member = await FindMemberAsync(projectId, userId);
            if (member is not null)
            {
                _db.ProjectMembers.Remove(member);
                await _db.SaveChangesAsync();
            }
        }

        // ── Stars ─────────────────────────────────────────────────────────────

        public async Task<ProjectStar?> FindStarAsync(int projectId, int userId) =>
            await _db.ProjectStars
                .FirstOrDefaultAsync(s =>
                    s.ProjectId == projectId && s.UserId == userId);

        public async Task<ProjectStar> AddStarAsync(ProjectStar star)
        {
            _db.ProjectStars.Add(star);
            await _db.SaveChangesAsync();
            return star;
        }

        public async Task RemoveStarAsync(int projectId, int userId)
        {
            var star = await FindStarAsync(projectId, userId);
            if (star is not null)
            {
                _db.ProjectStars.Remove(star);
                await _db.SaveChangesAsync();
            }
        }
    }
}