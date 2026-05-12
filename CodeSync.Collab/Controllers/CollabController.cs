using CodeSync.Collab.DTOs;
using CodeSync.Collab.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSync.Collab.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    public class CollabController : ControllerBase
    {
        private readonly ICollabService _collabService;

        public CollabController(ICollabService collabService)
        {
            _collabService = collabService;
        }

        // POST /api/sessions
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(
            [FromBody] CreateSessionRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _collabService
                    .CreateSessionAsync(userId.Value, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/sessions/{sessionId}
        [HttpGet("{sessionId:guid}")]
        public async Task<IActionResult> GetById(Guid sessionId)
        {
            try
            {
                var result = await _collabService
                    .GetSessionByIdAsync(sessionId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET /api/sessions/project/{projectId}
        [HttpGet("project/{projectId:int}")]
        public async Task<IActionResult> GetByProject(int projectId)
            => Ok(await _collabService.GetSessionsByProjectAsync(projectId));

        // GET /api/sessions/project/{projectId}/active
        [HttpGet("project/{projectId:int}/active")]
        public async Task<IActionResult> GetActiveByProject(int projectId)
            => Ok(await _collabService
                .GetActiveSessionsByProjectAsync(projectId));

        // GET /api/sessions/file/{fileId}/active
        [HttpGet("file/{fileId:int}/active")]
        public async Task<IActionResult> GetActiveByFile(int fileId)
        {
            var result = await _collabService
                .GetActiveSessionByFileAsync(fileId);
            if (result is null)
                return NotFound(new
                {
                    message = "No active session for this file."
                });
            return Ok(result);
        }

        // POST /api/sessions/join
        [HttpPost("join")]
        [Authorize]
        public async Task<IActionResult> Join(
            [FromBody] JoinSessionRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _collabService
                    .JoinSessionAsync(userId.Value, request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/sessions/{sessionId}/leave
        [HttpPost("{sessionId:guid}/leave")]
        [Authorize]
        public async Task<IActionResult> Leave(Guid sessionId)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            await _collabService.LeaveSessionAsync(sessionId, userId.Value);
            return Ok(new { message = "Left session." });
        }

        // POST /api/sessions/{sessionId}/end
        [HttpPost("{sessionId:guid}/end")]
        [Authorize]
        public async Task<IActionResult> End(Guid sessionId)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                await _collabService.EndSessionAsync(sessionId, userId.Value);
                return Ok(new { message = "Session ended." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // POST /api/sessions/{sessionId}/kick/{targetUserId}
        [HttpPost("{sessionId:guid}/kick/{targetUserId:int}")]
        [Authorize]
        public async Task<IActionResult> Kick(
            Guid sessionId, int targetUserId)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                await _collabService.KickParticipantAsync(
                    sessionId, userId.Value, targetUserId);
                return Ok(new { message = "Participant kicked." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // GET /api/sessions/{sessionId}/participants
        [HttpGet("{sessionId:guid}/participants")]
        public async Task<IActionResult> GetParticipants(Guid sessionId)
            => Ok(await _collabService.GetParticipantsAsync(sessionId));

        // PUT /api/sessions/{sessionId}/cursor
        [HttpPut("{sessionId:guid}/cursor")]
        [Authorize]
        public async Task<IActionResult> UpdateCursor(
            Guid sessionId, [FromBody] UpdateCursorRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            await _collabService.UpdateCursorAsync(
                sessionId, userId.Value, request.Line, request.Col);

            return Ok(new { message = "Cursor updated." });
        }

        private int? GetCurrentUserId()
        {
            var claim = User.Claims
                .FirstOrDefault(c => c.Type == "userId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}