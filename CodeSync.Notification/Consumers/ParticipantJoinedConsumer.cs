using CodeSync.Events.Collab;
using CodeSync.Notification.DTOs;
using CodeSync.Notification.Services;
using MassTransit;

namespace CodeSync.Notification.Consumers
{
    // ✅ SAGA Consumer
    // When participant joins → notify session owner
    public class ParticipantJoinedConsumer : IConsumer<ParticipantJoined>
    {
        private readonly INotificationService _notifService;
        private readonly ILogger<ParticipantJoinedConsumer> _logger;

        public ParticipantJoinedConsumer(
            INotificationService notifService,
            ILogger<ParticipantJoinedConsumer> logger)
        {
            _notifService = notifService;
            _logger       = logger;
        }

        public async Task Consume(ConsumeContext<ParticipantJoined> context)
        {
            var evt = context.Message;

            await _notifService.SendAsync(new SendNotificationRequest
            {
                RecipientId = evt.UserId,
                ActorId     = evt.UserId,
                Type        = "SESSION_INVITE",
                Title       = "Joined Collaboration Session",
                Message     = $"You joined a collaboration session.",
                RelatedId   = evt.SessionId.ToString(),
                RelatedType = "SESSION"
            });
        }
    }
}