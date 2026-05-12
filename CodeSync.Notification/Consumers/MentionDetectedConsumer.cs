using CodeSync.Events.Comments;
using CodeSync.Notification.DTOs;
using CodeSync.Notification.Services;
using MassTransit;

namespace CodeSync.Notification.Consumers
{
    // ✅ SAGA Consumer
    // When @mention detected in comment → notify mentioned users
    public class MentionDetectedConsumer : IConsumer<MentionDetected>
    {
        private readonly INotificationService _notifService;
        private readonly ILogger<MentionDetectedConsumer> _logger;

        public MentionDetectedConsumer(
            INotificationService notifService,
            ILogger<MentionDetectedConsumer> logger)
        {
            _notifService = notifService;
            _logger       = logger;
        }

        public async Task Consume(ConsumeContext<MentionDetected> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "MentionDetected received. Mentions={Count}",
                evt.MentionedUsernames.Count);

            // Note: In production you would look up user IDs by username
            // For now we log the mentions
            foreach (var username in evt.MentionedUsernames)
            {
                _logger.LogInformation(
                    "User @{Username} was mentioned in CommentId={CommentId}",
                    username, evt.CommentId);
            }
        }
    }
}