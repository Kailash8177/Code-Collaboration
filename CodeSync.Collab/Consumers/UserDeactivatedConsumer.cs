using CodeSync.Collab.Repositories;
using CodeSync.Events.Users;
using MassTransit;

namespace CodeSync.Collab.Consumers
{
    // ✅ SAGA Consumer
    // When user is deactivated → kick from all active sessions
    public class UserDeactivatedConsumer : IConsumer<UserDeactivated>
    {
        private readonly ICollabRepository _repo;
        private readonly ILogger<UserDeactivatedConsumer> _logger;

        public UserDeactivatedConsumer(
            ICollabRepository repo,
            ILogger<UserDeactivatedConsumer> logger)
        {
            _repo   = repo;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserDeactivated> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "UserDeactivated received — removing UserId={UserId} from sessions",
                evt.UserId);

            var sessions = await _repo.FindByOwnerIdAsync(evt.UserId);

            foreach (var session in sessions.Where(s => s.Status == "ACTIVE"))
            {
                var participant = await _repo
                    .FindParticipantAsync(session.SessionId, evt.UserId);

                if (participant is not null && participant.IsActive)
                {
                    participant.IsActive = false;
                    participant.LeftAt   = DateTime.UtcNow;
                    await _repo.UpdateParticipantAsync(participant);
                }
            }

            _logger.LogInformation(
                "UserId={UserId} removed from all active sessions", evt.UserId);
        }
    }
}