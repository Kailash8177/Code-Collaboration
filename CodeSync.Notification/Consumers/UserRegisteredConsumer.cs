using CodeSync.Events.Users;
using CodeSync.Notification.DTOs;
using CodeSync.Notification.Services;
using MassTransit;

namespace CodeSync.Notification.Consumers
{
    // ✅ SAGA Consumer
    // When user registers → send welcome notification
    public class UserRegisteredConsumer : IConsumer<UserRegistered>
    {
        private readonly INotificationService _notifService;
        private readonly ILogger<UserRegisteredConsumer> _logger;

        public UserRegisteredConsumer(
            INotificationService notifService,
            ILogger<UserRegisteredConsumer> logger)
        {
            _notifService = notifService;
            _logger       = logger;
        }

        public async Task Consume(ConsumeContext<UserRegistered> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "UserRegistered received for UserId={UserId}", evt.UserId);

            await _notifService.SendAsync(new SendNotificationRequest
            {
                RecipientId = evt.UserId,
                ActorId     = evt.UserId,
                Type        = "SYSTEM",
                Title       = "Welcome to CodeSync! 🎉",
                Message     = $"Hi {evt.FullName}! Your account has been created successfully.",
                RelatedId   = evt.UserId.ToString(),
                RelatedType = "USER",
                SendEmail   = true
            });
        }
    }
}