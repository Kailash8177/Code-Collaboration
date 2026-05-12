using CodeSync.Events.Collab;
using CodeSync.Notification.DTOs;
using CodeSync.Notification.Services;
using MassTransit;

namespace CodeSync.Notification.Consumers
{
    // ✅ SAGA Consumer
    // When participant kicked → notify kicked user
    public class ParticipantKickedConsumer : IConsumer<ParticipantKicked>
    {
        private readonly INotificationService _notifService;
        private readonly ILogger<ParticipantKickedConsumer> _logger;

        public ParticipantKickedConsumer(
            INotificationService notifService,
            ILogger<ParticipantKickedConsumer> logger)
        {
            _notifService = notifService;
            _logger       = logger;
        }

        public async Task Consume(ConsumeContext<ParticipantKicked> context)
        {
            var evt = context.Message;

            await _notifService.SendAsync(new SendNotificationRequest
            {
                RecipientId = evt.KickedUserId,
                ActorId     = evt.KickedByUserId,
                Type        = "SESSION_INVITE",
                Title       = "Removed from Session",
                Message     = "You were removed from the collaboration session.",
                RelatedId   = evt.SessionId.ToString(),
                RelatedType = "SESSION"
            });
        }
    }
}