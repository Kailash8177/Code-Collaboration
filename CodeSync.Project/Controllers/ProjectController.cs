using CodeSync.Project.DTOs;
using CodeSync.Project.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSync.Project.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        // POST /api/projects
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(
            [FromBody] CreateProjectRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _projectService
                    .CreateProjectAsync(userId.Value, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/projects/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetCurrentUserId();
            try
            {
                var result = await _projectService
                    .GetProjectByIdAsync(id, userId);
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

        // GET /api/projects/public
        [HttpGet("public")]
        public async Task<IActionResult> GetPublic()
            => Ok(await _projectService.GetPublicProjectsAsync());

        // GET /api/projects/admin/all
        [HttpGet("admin/all")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllAdmin()
            => Ok(await _projectService.GetAllProjectsAdminAsync());

        // GET /api/projects/owner/{ownerId}
        [HttpGet("owner/{ownerId:int}")]
        public async Task<IActionResult> GetByOwner(int ownerId)
            => Ok(await _projectService.GetProjectsByOwnerAsync(ownerId));

        // GET /api/projects/member/{userId}
        [HttpGet("member/{userId:int}")]
        [Authorize]
        public async Task<IActionResult> GetByMember(int userId)
            => Ok(await _projectService.GetProjectsByMemberAsync(userId));

        // GET /api/projects/language/{language}
        [HttpGet("language/{language}")]
        public async Task<IActionResult> GetByLanguage(string language)
            => Ok(await _projectService.GetProjectsByLanguageAsync(language));

        // GET /api/projects/search?q=
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { message = "Query is required." });
            return Ok(await _projectService.SearchProjectsAsync(q));
        }

        // PUT /api/projects/{id}
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateProjectRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _projectService
                    .UpdateProjectAsync(id, userId.Value, request);
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

        // PUT /api/projects/{id}/archive
        [HttpPut("{id:int}/archive")]
        [Authorize]
        public async Task<IActionResult> Archive(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                await _projectService.ArchiveProjectAsync(id, userId.Value);
                return Ok(new { message = "Project archived." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // DELETE /api/projects/{id}
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                await _projectService.DeleteProjectAsync(id, userId.Value);
                return Ok(new { message = "Project deleted." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // DELETE /api/projects/admin/{id}
        [HttpDelete("admin/{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            try
            {
                await _projectService.DeleteProjectAdminAsync(id);
                return Ok(new { message = "Project deleted by admin." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST /api/projects/{id}/fork
        [HttpPost("{id:int}/fork")]
        [Authorize]
        public async Task<IActionResult> Fork(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _projectService
                    .ForkProjectAsync(id, userId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/projects/{id}/star
        [HttpPost("{id:int}/star")]
        [Authorize]
        public async Task<IActionResult> Star(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            await _projectService.StarProjectAsync(id, userId.Value);
            return Ok(new { message = "Project starred." });
        }

        // DELETE /api/projects/{id}/star
        [HttpDelete("{id:int}/star")]
        [Authorize]
        public async Task<IActionResult> Unstar(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            await _projectService.UnstarProjectAsync(id, userId.Value);
            return Ok(new { message = "Project unstarred." });
        }

        // GET /api/projects/{id}/members
        [HttpGet("{id:int}/members")]
        public async Task<IActionResult> GetMembers(int id)
            => Ok(await _projectService.GetMembersAsync(id));

        // POST /api/projects/{id}/members
        [HttpPost("{id:int}/members")]
        [Authorize]
        public async Task<IActionResult> AddMember(
            int id, [FromBody] AddMemberRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                await _projectService.AddMemberAsync(
                    id, userId.Value, request);
                return Ok(new { message = "Member added." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // DELETE /api/projects/{id}/members/{targetUserId}
        [HttpDelete("{id:int}/members/{targetUserId:int}")]
        [Authorize]
        public async Task<IActionResult> RemoveMember(int id, int targetUserId)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                await _projectService.RemoveMemberAsync(
                    id, userId.Value, targetUserId);
                return Ok(new { message = "Member removed." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private int? GetCurrentUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}