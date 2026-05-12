using CodeSync.Notification.DTOs;
using CodeSync.Notification.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSync.Notification.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notifService;

        public NotificationController(INotificationService notifService)
        {
            _notifService = notifService;
        }

        // POST /api/notifications/send
        [HttpPost("send")]
        [AllowAnonymous]
        public async Task<IActionResult> Send(
            [FromBody] SendNotificationRequest request)
        {
            try
            {
                var result = await _notifService.SendAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/notifications/bulk
        [HttpPost("bulk")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> SendBulk(
            [FromBody] SendBulkNotificationRequest request)
        {
            await _notifService.SendBulkAsync(request);
            return Ok(new { message = "Bulk notification sent." });
        }

        // GET /api/notifications/recipient/{recipientId}
        [HttpGet("recipient/{recipientId:int}")]
        [Authorize]
        public async Task<IActionResult> GetByRecipient(int recipientId)
            => Ok(await _notifService.GetByRecipientAsync(recipientId));

        // GET /api/notifications/recipient/{recipientId}/unread
        [HttpGet("recipient/{recipientId:int}/unread")]
        [Authorize]
        public async Task<IActionResult> GetUnread(int recipientId)
            => Ok(await _notifService.GetUnreadAsync(recipientId));

        // GET /api/notifications/recipient/{recipientId}/count
        [HttpGet("recipient/{recipientId:int}/count")]
        [Authorize]
        public async Task<IActionResult> GetUnreadCount(int recipientId)
            => Ok(await _notifService.GetUnreadCountAsync(recipientId));

        // PUT /api/notifications/{id}/read
        [HttpPut("{id:int}/read")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                await _notifService.MarkAsReadAsync(id, userId.Value);
                return Ok(new { message = "Marked as read." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT /api/notifications/recipient/{recipientId}/read-all
        [HttpPut("recipient/{recipientId:int}/read-all")]
        [Authorize]
        public async Task<IActionResult> MarkAllRead(int recipientId)
        {
            await _notifService.MarkAllReadAsync(recipientId);
            return Ok(new { message = "All marked as read." });
        }

        // DELETE /api/notifications/{id}
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                await _notifService.DeleteAsync(id, userId.Value);
                return Ok(new { message = "Notification deleted." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE /api/notifications/recipient/{recipientId}/read
        [HttpDelete("recipient/{recipientId:int}/read")]
        [Authorize]
        public async Task<IActionResult> DeleteAllRead(int recipientId)
        {
            await _notifService.DeleteAllReadAsync(recipientId);
            return Ok(new { message = "All read notifications deleted." });
        }

        private int? GetCurrentUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}