using CodeSync.Events.Version;
using CodeSync.Notification.DTOs;
using CodeSync.Notification.Services;
using MassTransit;

namespace CodeSync.Notification.Consumers
{
    // ✅ SAGA Consumer
    // When snapshot created → notify collaborators
    public class SnapshotCreatedConsumer : IConsumer<SnapshotCreated>
    {
        private readonly INotificationService _notifService;
        private readonly ILogger<SnapshotCreatedConsumer> _logger;

        public SnapshotCreatedConsumer(
            INotificationService notifService,
            ILogger<SnapshotCreatedConsumer> logger)
        {
            _notifService = notifService;
            _logger       = logger;
        }

        public async Task Consume(ConsumeContext<SnapshotCreated> context)
        {
            var evt = context.Message;

            await _notifService.SendAsync(new SendNotificationRequest
            {
                RecipientId = evt.AuthorId,
                ActorId     = evt.AuthorId,
                Type        = "SNAPSHOT",
                Title       = "Snapshot Created",
                Message     = $"Snapshot created: {evt.Message}",
                RelatedId   = evt.SnapshotId.ToString(),
                RelatedType = "SNAPSHOT"
            });
        }
    }
}