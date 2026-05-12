using CodeSync.Events.Collab;
using CodeSync.Notification.DTOs;
using CodeSync.Notification.Services;
using MassTransit;

namespace CodeSync.Notification.Consumers
{
    // ✅ SAGA Consumer
    // When session starts → notify owner
    public class SessionStartedConsumer : IConsumer<SessionStarted>
    {
        private readonly INotificationService _notifService;
        private readonly ILogger<SessionStartedConsumer> _logger;

        public SessionStartedConsumer(
            INotificationService notifService,
            ILogger<SessionStartedConsumer> logger)
        {
            _notifService = notifService;
            _logger       = logger;
        }

        public async Task Consume(ConsumeContext<SessionStarted> context)
        {
            var evt = context.Message;

            await _notifService.SendAsync(new SendNotificationRequest
            {
                RecipientId = evt.OwnerId,
                ActorId     = evt.OwnerId,
                Type        = "SESSION_INVITE",
                Title       = "Collaboration Session Started",
                Message     = $"Your session has started for file {evt.FileId}.",
                RelatedId   = evt.SessionId.ToString(),
                RelatedType = "SESSION"
            });
        }
    }
}