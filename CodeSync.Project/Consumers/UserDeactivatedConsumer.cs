using CodeSync.Events.Users;
using CodeSync.Project.Repositories;
using MassTransit;

namespace CodeSync.Project.Consumers
{
    // ✅ SAGA Consumer
    // Listens for UserDeactivated event
    // Removes deactivated user from ALL projects
    public class UserDeactivatedConsumer : IConsumer<UserDeactivated>
    {
        private readonly IProjectRepository _repo;
        private readonly ILogger<UserDeactivatedConsumer> _logger;

        public UserDeactivatedConsumer(
            IProjectRepository repo,
            ILogger<UserDeactivatedConsumer> logger)
        {
            _repo   = repo;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserDeactivated> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "UserDeactivated received — removing UserId={UserId} from all projects",
                evt.UserId);

            // Find all projects where user is a member
            var projects = await _repo.FindByMemberUserIdAsync(evt.UserId);

            foreach (var project in projects)
            {
                // Do not remove if user is the owner
                // Owner deactivation is handled separately by admin
                var member = await _repo.FindMemberAsync(
                    project.ProjectId, evt.UserId);

                if (member is not null && member.Role != "OWNER")
                {
                    await _repo.RemoveMemberAsync(
                        project.ProjectId, evt.UserId);

                    _logger.LogInformation(
                        "Removed UserId={UserId} from ProjectId={ProjectId}",
                        evt.UserId, project.ProjectId);
                }
            }
        }
    }
}