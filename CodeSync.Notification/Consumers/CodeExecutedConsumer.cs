using CodeSync.Events.Execution;
using CodeSync.Notification.DTOs;
using CodeSync.Notification.Services;
using MassTransit;

namespace CodeSync.Notification.Consumers
{
    // ✅ SAGA Consumer
    // When code execution finishes → notify user
    public class CodeExecutedConsumer : IConsumer<CodeExecuted>
    {
        private readonly INotificationService _notifService;
        private readonly ILogger<CodeExecutedConsumer> _logger;

        public CodeExecutedConsumer(
            INotificationService notifService,
            ILogger<CodeExecutedConsumer> logger)
        {
            _notifService = notifService;
            _logger       = logger;
        }

        public async Task Consume(ConsumeContext<CodeExecuted> context)
        {
            var evt = context.Message;

            await _notifService.SendAsync(new SendNotificationRequest
            {
                RecipientId = evt.UserId,
                ActorId     = evt.UserId,
                Type        = "EXECUTION",
                Title       = $"Code Execution {evt.Status}",
                Message     = $"Your {evt.Language} code finished in {evt.ExecutionTimeMs}ms.",
                RelatedId   = evt.JobId.ToString(),
                RelatedType = "EXECUTION"
            });
        }
    }
}