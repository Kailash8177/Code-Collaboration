using CodeSync.Collab.DTOs;
using CodeSync.Collab.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CodeSync.Collab.Hubs
{
    [Authorize]
    public class CollabHub : Hub
    {
        private readonly ICollabService _collabService;

        public CollabHub(ICollabService collabService)
        {
            _collabService = collabService;
        }

        // Called when user joins a session room
        public async Task JoinSessionRoom(string sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

            var userId = GetCurrentUserId();

            await Clients.OthersInGroup(sessionId)
                .SendAsync("ParticipantJoined", new ParticipantJoinedPayload
                {
                    UserId = userId,
                    Color  = "#33FF57",
                    Role   = "EDITOR"
                });
        }

        // Called when user leaves a session room
        public async Task LeaveSessionRoom(string sessionId)
        {
            var userId = GetCurrentUserId();

            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, sessionId);

            await Clients.OthersInGroup(sessionId)
                .SendAsync("ParticipantLeft", new { UserId = userId });

            if (Guid.TryParse(sessionId, out var guid))
                await _collabService.LeaveSessionAsync(guid, userId);
        }

        // Called when user types code
        public async Task SendCodeChange(
            string sessionId, string content, string changeType)
        {
            var userId = GetCurrentUserId();

            if (Guid.TryParse(sessionId, out var guid))
                await _collabService.UpdateLastActivityAsync(guid);

            await Clients.OthersInGroup(sessionId)
                .SendAsync("ReceiveCodeChange", new EditorChangePayload
                {
                    UserId     = userId,
                    Content    = content,
                    ChangeType = changeType,
                    Timestamp  = DateTime.UtcNow
                });
        }

        // Called when user moves cursor
        public async Task SendCursorUpdate(
            string sessionId, int line, int col)
        {
            var userId = GetCurrentUserId();

            if (Guid.TryParse(sessionId, out var guid))
                await _collabService.UpdateCursorAsync(guid, userId, line, col);

            await Clients.OthersInGroup(sessionId)
                .SendAsync("ReceiveCursorUpdate", new CursorUpdatePayload
                {
                    UserId = userId,
                    Line   = line,
                    Col    = col,
                    Color  = "#FF5733"
                });
        }

        // Called when connection drops
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            await Clients.All.SendAsync(
                "ParticipantLeft", new { UserId = userId });
            await base.OnDisconnectedAsync(exception);
        }

        private int GetCurrentUserId()
        {
            var claim = Context.User?.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}