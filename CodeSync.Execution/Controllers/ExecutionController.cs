using CodeSync.Execution.DTOs;
using CodeSync.Execution.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSync.Execution.Controllers
{
    [ApiController]
    [Route("api/executions")]
    public class ExecutionController : ControllerBase
    {
        private readonly IExecutionService _executionService;

        public ExecutionController(IExecutionService executionService)
        {
            _executionService = executionService;
        }

        // POST /api/executions/submit
        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> Submit(
            [FromBody] SubmitExecutionRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _executionService
                    .SubmitExecutionAsync(userId.Value, request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/executions/{jobId}
        [HttpGet("{jobId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid jobId)
        {
            try
            {
                var result = await _executionService.GetJobByIdAsync(jobId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET /api/executions/{jobId}/result
        [HttpGet("{jobId:guid}/result")]
        [Authorize]
        public async Task<IActionResult> GetResult(Guid jobId)
        {
            try
            {
                var result = await _executionService
                    .GetExecutionResultAsync(jobId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET /api/executions/user/{userId}
        [HttpGet("user/{userId:int}")]
        [Authorize]
        public async Task<IActionResult> GetByUser(int userId)
            => Ok(await _executionService.GetExecutionsByUserAsync(userId));

        // GET /api/executions/project/{projectId}
        [HttpGet("project/{projectId:int}")]
        [Authorize]
        public async Task<IActionResult> GetByProject(int projectId)
            => Ok(await _executionService
                .GetExecutionsByProjectAsync(projectId));

        // POST /api/executions/{jobId}/cancel
        [HttpPost("{jobId:guid}/cancel")]
        [Authorize]
        public async Task<IActionResult> Cancel(Guid jobId)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                await _executionService.CancelExecutionAsync(
                    jobId, userId.Value);
                return Ok(new { message = "Execution cancelled." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/executions/languages
        [HttpGet("languages")]
        public async Task<IActionResult> GetLanguages()
            => Ok(await _executionService.GetSupportedLanguagesAsync());

        // GET /api/executions/stats
        [HttpGet("stats")]
        [Authorize]
        public async Task<IActionResult> GetStats()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            var result = await _executionService
                .GetExecutionStatsAsync(userId.Value);
            return Ok(result);
        }

        private int? GetCurrentUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}