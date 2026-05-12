using CodeSync.Events.Projects;
using CodeSync.Notification.DTOs;
using CodeSync.Notification.Services;
using MassTransit;

namespace CodeSync.Notification.Consumers
{
    // ✅ SAGA Consumer
    // When member added to project → notify new member
    public class MemberAddedConsumer : IConsumer<MemberAdded>
    {
        private readonly INotificationService _notifService;
        private readonly ILogger<MemberAddedConsumer> _logger;

        public MemberAddedConsumer(
            INotificationService notifService,
            ILogger<MemberAddedConsumer> logger)
        {
            _notifService = notifService;
            _logger       = logger;
        }

        public async Task Consume(ConsumeContext<MemberAdded> context)
        {
            var evt = context.Message;

            await _notifService.SendAsync(new SendNotificationRequest
            {
                RecipientId = evt.UserId,
                ActorId     = evt.AddedByUserId,
                Type        = "MEMBER_ADDED",
                Title       = "Added to Project",
                Message     = $"You have been added to project as {evt.Role}.",
                RelatedId   = evt.ProjectId.ToString(),
                RelatedType = "PROJECT"
            });
        }
    }
}