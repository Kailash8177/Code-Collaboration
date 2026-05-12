using CodeSync.ReviewHub.DTOs;
using CodeSync.ReviewHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSync.ReviewHub.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // POST /api/comments
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add(
            [FromBody] AddCommentRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _commentService
                    .AddCommentAsync(userId.Value, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/comments/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _commentService.GetCommentByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET /api/comments/file/{fileId}
        [HttpGet("file/{fileId:int}")]
        public async Task<IActionResult> GetByFile(int fileId)
            => Ok(await _commentService.GetCommentsByFileAsync(fileId));

        // GET /api/comments/project/{projectId}
        [HttpGet("project/{projectId:int}")]
        public async Task<IActionResult> GetByProject(int projectId)
            => Ok(await _commentService.GetCommentsByProjectAsync(projectId));

        // GET /api/comments/file/{fileId}/line/{lineNumber}
        [HttpGet("file/{fileId:int}/line/{lineNumber:int}")]
        public async Task<IActionResult> GetByLine(int fileId, int lineNumber)
            => Ok(await _commentService
                .GetCommentsByLineAsync(fileId, lineNumber));

        // GET /api/comments/{id}/replies
        [HttpGet("{id:int}/replies")]
        public async Task<IActionResult> GetReplies(int id)
            => Ok(await _commentService.GetRepliesAsync(id));

        // GET /api/comments/file/{fileId}/count
        [HttpGet("file/{fileId:int}/count")]
        public async Task<IActionResult> GetCount(int fileId)
        {
            var count = await _commentService.GetCommentCountAsync(fileId);
            return Ok(new { fileId, count });
        }

        // PUT /api/comments/{id}
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateCommentRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _commentService
                    .UpdateCommentAsync(id, userId.Value, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // DELETE /api/comments/{id}
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                await _commentService.DeleteCommentAsync(id, userId.Value);
                return Ok(new { message = "Comment deleted." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // POST /api/comments/{id}/resolve
        [HttpPost("{id:int}/resolve")]
        [Authorize]
        public async Task<IActionResult> Resolve(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            await _commentService.ResolveCommentAsync(id, userId.Value);
            return Ok(new { message = "Comment resolved." });
        }

        // POST /api/comments/{id}/unresolve
        [HttpPost("{id:int}/unresolve")]
        [Authorize]
        public async Task<IActionResult> Unresolve(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            await _commentService.UnresolveCommentAsync(id, userId.Value);
            return Ok(new { message = "Comment unresolved." });
        }

        private int? GetCurrentUserId()
        {
            var claim = User.Claims
                .FirstOrDefault(c => c.Type == "userId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}