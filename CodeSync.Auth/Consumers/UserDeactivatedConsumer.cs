using CodeSync.Auth.Data;
using CodeSync.Events.Users;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.Auth.Consumers
{
    // ✅ SAGA Consumer
    // Listens for UserDeactivated event
    // Revokes ALL refresh tokens for that user
    public class UserDeactivatedConsumer : IConsumer<UserDeactivated>
    {
        private readonly AuthDbContext _db;
        private readonly ILogger<UserDeactivatedConsumer> _logger;

        public UserDeactivatedConsumer(
            AuthDbContext db,
            ILogger<UserDeactivatedConsumer> logger)
        {
            _db     = db;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserDeactivated> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "UserDeactivated received — revoking tokens for UserId={UserId}",
                evt.UserId);

            // Revoke all active refresh tokens
            var tokens = _db.RefreshTokens
                .Where(t => t.UserId == evt.UserId && !t.IsRevoked);

            await tokens.ForEachAsync(t => t.IsRevoked = true);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "All tokens revoked for UserId={UserId}", evt.UserId);
        }
    }
}