using CodeSync.ReviewHub.DTOs;
using CodeSync.ReviewHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSync.ReviewHub.Controllers
{
    [ApiController]
    [Route("api/snapshots")]
    public class SnapshotController : ControllerBase
    {
        private readonly ISnapshotService _snapshotService;

        public SnapshotController(ISnapshotService snapshotService)
        {
            _snapshotService = snapshotService;
        }

        // POST /api/snapshots
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(
            [FromBody] CreateSnapshotRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _snapshotService
                    .CreateSnapshotAsync(userId.Value, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/snapshots/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _snapshotService.GetSnapshotByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET /api/snapshots/file/{fileId}
        [HttpGet("file/{fileId:int}")]
        public async Task<IActionResult> GetByFile(int fileId)
            => Ok(await _snapshotService.GetSnapshotsByFileAsync(fileId));

        // GET /api/snapshots/project/{projectId}
        [HttpGet("project/{projectId:int}")]
        public async Task<IActionResult> GetByProject(int projectId)
            => Ok(await _snapshotService.GetSnapshotsByProjectAsync(projectId));

        // GET /api/snapshots/project/{projectId}/branch/{branch}
        [HttpGet("project/{projectId:int}/branch/{branch}")]
        public async Task<IActionResult> GetByBranch(int projectId, string branch)
            => Ok(await _snapshotService
                .GetSnapshotsByBranchAsync(projectId, branch));

        // GET /api/snapshots/file/{fileId}/latest?branch=main
        [HttpGet("file/{fileId:int}/latest")]
        public async Task<IActionResult> GetLatest(
            int fileId, [FromQuery] string branch = "main")
        {
            var result = await _snapshotService
                .GetLatestSnapshotAsync(fileId, branch);
            if (result is null)
                return NotFound(new { message = "No snapshot found." });
            return Ok(result);
        }

        // GET /api/snapshots/project/{projectId}/branches
        [HttpGet("project/{projectId:int}/branches")]
        public async Task<IActionResult> GetBranches(int projectId)
            => Ok(await _snapshotService.GetBranchesAsync(projectId));

        // GET /api/snapshots/diff/{id1}/{id2}
        [HttpGet("diff/{id1:int}/{id2:int}")]
        public async Task<IActionResult> Diff(int id1, int id2)
        {
            try
            {
                var result = await _snapshotService.DiffSnapshotsAsync(id1, id2);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST /api/snapshots/{id}/restore
        [HttpPost("{id:int}/restore")]
        [Authorize]
        public async Task<IActionResult> Restore(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _snapshotService
                    .RestoreSnapshotAsync(id, userId.Value);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/snapshots/{id}/tag
        [HttpPut("{id:int}/tag")]
        [Authorize]
        public async Task<IActionResult> Tag(
            int id, [FromBody] TagSnapshotRequest request)
        {
            try
            {
                await _snapshotService.TagSnapshotAsync(id, request);
                return Ok(new { message = "Snapshot tagged." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private int? GetCurrentUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}