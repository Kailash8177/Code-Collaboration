using CodeSync.Events.Projects;
using CodeSync.Notification.DTOs;
using CodeSync.Notification.Services;
using MassTransit;

namespace CodeSync.Notification.Consumers
{
    // ✅ SAGA Consumer
    // When project is forked → notify original owner
    public class ProjectForkedConsumer : IConsumer<ProjectForked>
    {
        private readonly INotificationService _notifService;
        private readonly ILogger<ProjectForkedConsumer> _logger;

        public ProjectForkedConsumer(
            INotificationService notifService,
            ILogger<ProjectForkedConsumer> logger)
        {
            _notifService = notifService;
            _logger       = logger;
        }

        public async Task Consume(ConsumeContext<ProjectForked> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "ProjectForked received. OriginalId={OriginalId}",
                evt.OriginalProjectId);

            await _notifService.SendAsync(new SendNotificationRequest
            {
                RecipientId = evt.ForkedByUserId,
                ActorId     = evt.ForkedByUserId,
                Type        = "FORK",
                Title       = "Project Forked",
                Message     = $"Your project was forked successfully.",
                RelatedId   = evt.ForkedProjectId.ToString(),
                RelatedType = "PROJECT"
            });
        }
    }
}