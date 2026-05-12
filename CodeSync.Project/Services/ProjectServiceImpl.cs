using CodeSync.Events.Projects;
using CodeSync.Project.DTOs;
using CodeSync.Project.Models;
using CodeSync.Project.Repositories;
using MassTransit;

namespace CodeSync.Project.Services
{
    public class ProjectServiceImpl : IProjectService
    {
        private readonly IProjectRepository _repo;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ProjectServiceImpl> _logger;

        public ProjectServiceImpl(
            IProjectRepository repo,
            IPublishEndpoint publishEndpoint,
            ILogger<ProjectServiceImpl> logger)
        {
            _repo            = repo;
            _publishEndpoint = publishEndpoint;
            _logger          = logger;
        }

        // ── Create Project ────────────────────────────────────────────────────

        public async Task<ProjectResponse> CreateProjectAsync(
            int ownerId, CreateProjectRequest req)
        {
            var project = new Models.Project
            {
                OwnerId     = ownerId,
                Name        = req.Name,
                Description = req.Description,
                Language    = req.Language,
                Visibility  = req.Visibility.ToUpper()
            };

            await _repo.CreateAsync(project);

            // Owner automatically gets OWNER role
            await _repo.AddMemberAsync(new ProjectMember
            {
                ProjectId = project.ProjectId,
                UserId    = ownerId,
                Role      = "OWNER"
            });

            // ✅ SAGA — publish ProjectCreated event
            // File-Service listens        → creates README.md
            // Notification-Service listens → sends welcome message
            await _publishEndpoint.Publish(new ProjectCreated(
                ProjectId:  project.ProjectId,
                OwnerId:    ownerId,
                Name:       project.Name,
                Language:   project.Language,
                Visibility: project.Visibility,
                CreatedAt:  project.CreatedAt
            ));

            _logger.LogInformation(
                "ProjectCreated event published for ProjectId={ProjectId}",
                project.ProjectId);

            return MapToResponse(project);
        }

        // ── Get Project ───────────────────────────────────────────────────────

        public async Task<ProjectResponse> GetProjectByIdAsync(
            int projectId, int? requestingUserId)
        {
            var project = await _repo.FindByProjectIdAsync(projectId)
                ?? throw new KeyNotFoundException(
                    $"Project {projectId} not found.");

            if (project.Visibility == "PRIVATE" &&
                requestingUserId is not null)
            {
                var member = await _repo.FindMemberAsync(
                    projectId, requestingUserId.Value);
                if (member is null)
                    throw new UnauthorizedAccessException(
                        "Access denied to private project.");
            }

            return MapToResponse(project);
        }

        // ── Get Projects ──────────────────────────────────────────────────────

        public async Task<IEnumerable<ProjectResponse>> GetProjectsByOwnerAsync(
            int ownerId)
        {
            var projects = await _repo.FindByOwnerIdAsync(ownerId);
            return projects.Select(MapToResponse);
        }

        public async Task<IEnumerable<ProjectResponse>> GetPublicProjectsAsync()
        {
            var projects = await _repo.FindByVisibilityAsync("PUBLIC");
            return projects.Select(MapToResponse);
        }

        public async Task<IEnumerable<ProjectResponse>> GetAllProjectsAdminAsync()
        {
            var projects = await _repo.FindAllAsync();
            return projects.Select(MapToResponse);
        }

        public async Task<IEnumerable<ProjectResponse>> SearchProjectsAsync(
            string query)
        {
            var projects = await _repo.SearchByNameAsync(query);
            return projects.Select(MapToResponse);
        }

        public async Task<IEnumerable<ProjectResponse>> GetProjectsByMemberAsync(
            int userId)
        {
            var projects = await _repo.FindByMemberUserIdAsync(userId);
            return projects.Select(MapToResponse);
        }

        public async Task<IEnumerable<ProjectResponse>> GetProjectsByLanguageAsync(
            string language)
        {
            var projects = await _repo.FindByLanguageAsync(language);
            return projects.Select(MapToResponse);
        }

        // ── Update Project ────────────────────────────────────────────────────

        public async Task<ProjectResponse> UpdateProjectAsync(
            int projectId, int requestingUserId, UpdateProjectRequest req)
        {
            var project = await _repo.FindByProjectIdAsync(projectId)
                ?? throw new KeyNotFoundException(
                    $"Project {projectId} not found.");

            await EnsureOwnerAsync(projectId, requestingUserId);

            if (req.Name        is not null) project.Name        = req.Name;
            if (req.Description is not null) project.Description = req.Description;
            if (req.Language    is not null) project.Language    = req.Language;
            if (req.Visibility  is not null)
                project.Visibility = req.Visibility.ToUpper();

            await _repo.UpdateAsync(project);
            return MapToResponse(project);
        }

        // ── Archive Project ───────────────────────────────────────────────────

        public async Task ArchiveProjectAsync(int projectId, int requestingUserId)
        {
            var project = await _repo.FindByProjectIdAsync(projectId)
                ?? throw new KeyNotFoundException(
                    $"Project {projectId} not found.");

            await EnsureOwnerAsync(projectId, requestingUserId);

            project.IsArchived = true;
            await _repo.UpdateAsync(project);
        }

        // ── Delete Project ────────────────────────────────────────────────────

        public async Task DeleteProjectAsync(int projectId, int requestingUserId)
        {
            var project = await _repo.FindByProjectIdAsync(projectId)
                ?? throw new KeyNotFoundException(
                    $"Project {projectId} not found.");

            if (project.OwnerId != requestingUserId)
                throw new UnauthorizedAccessException(
                    "Only the owner can delete a project.");

            await _repo.DeleteAsync(projectId);

            // ✅ SAGA — publish ProjectDeleted event
            // File-Service listens    → soft deletes all files
            // Version-Service listens → archives all snapshots
            // Comment-Service listens → deletes all comments
            await _publishEndpoint.Publish(new ProjectDeleted(
                ProjectId: projectId,
                OwnerId:   requestingUserId,
                DeletedAt: DateTime.UtcNow
            ));

            _logger.LogInformation(
                "ProjectDeleted event published for ProjectId={ProjectId}",
                projectId);
        }

        public async Task DeleteProjectAdminAsync(int projectId)
        {
            var project = await _repo.FindByProjectIdAsync(projectId)
                ?? throw new KeyNotFoundException($"Project {projectId} not found.");

            await _repo.DeleteAsync(projectId);

            await _publishEndpoint.Publish(new ProjectDeleted(
                ProjectId: projectId,
                OwnerId:   project.OwnerId,
                DeletedAt: DateTime.UtcNow
            ));

            _logger.LogInformation("Admin ProjectDeleted event published for ProjectId={ProjectId}", projectId);
        }

        // ── Fork Project ──────────────────────────────────────────────────────

        public async Task<ProjectResponse> ForkProjectAsync(
            int projectId, int requestingUserId)
        {
            var original = await _repo.FindByProjectIdAsync(projectId)
                ?? throw new KeyNotFoundException(
                    $"Project {projectId} not found.");

            if (original.Visibility != "PUBLIC")
                throw new UnauthorizedAccessException(
                    "Can only fork public projects.");

            var forked = new Models.Project
            {
                OwnerId     = requestingUserId,
                Name        = $"{original.Name}-fork",
                Description = $"Forked from {original.Name}",
                Language    = original.Language,
                Visibility  = "PRIVATE"
            };

            await _repo.CreateAsync(forked);

            await _repo.AddMemberAsync(new ProjectMember
            {
                ProjectId = forked.ProjectId,
                UserId    = requestingUserId,
                Role      = "OWNER"
            });

            original.ForkCount++;
            await _repo.UpdateAsync(original);

            // ✅ SAGA — publish ProjectForked event
            // File-Service listens        → copies all files to new project
            // Notification-Service listens → notifies original owner
            await _publishEndpoint.Publish(new ProjectForked(
                OriginalProjectId: projectId,
                ForkedProjectId:   forked.ProjectId,
                ForkedByUserId:    requestingUserId,
                ForkedAt:          DateTime.UtcNow
            ));

            _logger.LogInformation(
                "ProjectForked event published. Original={OriginalId} Fork={ForkId}",
                projectId, forked.ProjectId);

            return MapToResponse(forked);
        }

        // ── Star Project ──────────────────────────────────────────────────────

        public async Task StarProjectAsync(int projectId, int userId)
        {
            var project = await _repo.FindByProjectIdAsync(projectId)
                ?? throw new KeyNotFoundException(
                    $"Project {projectId} not found.");

            var existing = await _repo.FindStarAsync(projectId, userId);
            if (existing is not null) return;

            await _repo.AddStarAsync(new ProjectStar
            {
                ProjectId = projectId,
                UserId    = userId
            });

            project.StarCount++;
            await _repo.UpdateAsync(project);
        }

        public async Task UnstarProjectAsync(int projectId, int userId)
        {
            var project = await _repo.FindByProjectIdAsync(projectId)
                ?? throw new KeyNotFoundException(
                    $"Project {projectId} not found.");

            var existing = await _repo.FindStarAsync(projectId, userId);
            if (existing is null) return;

            await _repo.RemoveStarAsync(projectId, userId);

            if (project.StarCount > 0) project.StarCount--;
            await _repo.UpdateAsync(project);
        }

        // ── Members ───────────────────────────────────────────────────────────

        public async Task<IEnumerable<ProjectMemberResponse>> GetMembersAsync(
            int projectId)
        {
            var members = await _repo.FindMembersByProjectIdAsync(projectId);
            return members.Select(m => new ProjectMemberResponse
            {
                UserId   = m.UserId,
                Role     = m.Role,
                JoinedAt = m.JoinedAt
            });
        }

        public async Task AddMemberAsync(
            int projectId, int requestingUserId, AddMemberRequest req)
        {
            await EnsureOwnerAsync(projectId, requestingUserId);

            var existing = await _repo.FindMemberAsync(projectId, req.UserId);
            if (existing is not null)
                throw new InvalidOperationException(
                    "User is already a member.");

            await _repo.AddMemberAsync(new ProjectMember
            {
                ProjectId = projectId,
                UserId    = req.UserId,
                Role      = req.Role.ToUpper()
            });

            // ✅ SAGA — publish MemberAdded event
            // Notification-Service listens → notifies new member
            await _publishEndpoint.Publish(new MemberAdded(
                ProjectId:     projectId,
                UserId:        req.UserId,
                Role:          req.Role.ToUpper(),
                AddedByUserId: requestingUserId,
                AddedAt:       DateTime.UtcNow
            ));

            _logger.LogInformation(
                "MemberAdded event published. ProjectId={ProjectId} UserId={UserId}",
                projectId, req.UserId);
        }

        public async Task RemoveMemberAsync(
            int projectId, int requestingUserId, int targetUserId)
        {
            await EnsureOwnerAsync(projectId, requestingUserId);
            await _repo.RemoveMemberAsync(projectId, targetUserId);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task EnsureOwnerAsync(int projectId, int userId)
        {
            var member = await _repo.FindMemberAsync(projectId, userId)
                ?? throw new UnauthorizedAccessException(
                    "You are not a member of this project.");

            if (member.Role != "OWNER")
                throw new UnauthorizedAccessException(
                    "Only the project owner can do this.");
        }

        private static ProjectResponse MapToResponse(Models.Project p) => new()
        {
            ProjectId   = p.ProjectId,
            OwnerId     = p.OwnerId,
            Name        = p.Name,
            Description = p.Description,
            Language    = p.Language,
            Visibility  = p.Visibility,
            IsArchived  = p.IsArchived,
            StarCount   = p.StarCount,
            ForkCount   = p.ForkCount,
            CreatedAt   = p.CreatedAt,
            UpdatedAt   = p.UpdatedAt
        };
    }
}