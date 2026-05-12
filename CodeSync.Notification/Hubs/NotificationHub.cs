using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CodeSync.Notification.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // When user connects → join their personal group
        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            if (userId > 0)
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId, $"user-{userId}");
            }
            await base.OnConnectedAsync();
        }

        // When user disconnects → leave their group
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            if (userId > 0)
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId, $"user-{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        private int GetCurrentUserId()
        {
            var claim = Context.User?.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}