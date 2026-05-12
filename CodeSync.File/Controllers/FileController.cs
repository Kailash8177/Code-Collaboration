using CodeSync.File.DTOs;
using CodeSync.File.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSync.File.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        // POST /api/files
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateFile(
            [FromBody] CreateFileRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _fileService
                    .CreateFileAsync(userId.Value, request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // POST /api/files/folder
        [HttpPost("folder")]
        [Authorize]
        public async Task<IActionResult> CreateFolder(
            [FromBody] CreateFolderRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _fileService
                    .CreateFolderAsync(userId.Value, request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // GET /api/files/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _fileService.GetFileByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET /api/files/{id}/content
        [HttpGet("{id:int}/content")]
        public async Task<IActionResult> GetContent(int id)
        {
            try
            {
                var result = await _fileService.GetFileContentAsync(id);
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

        // GET /api/files/project/{projectId}
        [HttpGet("project/{projectId:int}")]
        public async Task<IActionResult> GetByProject(int projectId)
            => Ok(await _fileService.GetFilesByProjectAsync(projectId));

        // GET /api/files/project/{projectId}/tree
        [HttpGet("project/{projectId:int}/tree")]
        public async Task<IActionResult> GetTree(int projectId)
            => Ok(await _fileService.GetFileTreeAsync(projectId));

        // GET /api/files/project/{projectId}/search?q=
        [HttpGet("project/{projectId:int}/search")]
        public async Task<IActionResult> Search(
            int projectId, [FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { message = "Search query is required." });

            var result = await _fileService
                .SearchInProjectAsync(projectId, q);
            return Ok(result);
        }

        // PUT /api/files/{id}/content
        [HttpPut("{id:int}/content")]
        [Authorize]
        public async Task<IActionResult> UpdateContent(
            int id, [FromBody] UpdateFileContentRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _fileService
                    .UpdateFileContentAsync(id, userId.Value, request);
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

        // PUT /api/files/{id}/rename
        [HttpPut("{id:int}/rename")]
        [Authorize]
        public async Task<IActionResult> Rename(
            int id, [FromBody] RenameFileRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _fileService
                    .RenameFileAsync(id, userId.Value, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT /api/files/{id}/move
        [HttpPut("{id:int}/move")]
        [Authorize]
        public async Task<IActionResult> Move(
            int id, [FromBody] MoveFileRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _fileService
                    .MoveFileAsync(id, userId.Value, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // DELETE /api/files/{id}
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                await _fileService.DeleteFileAsync(id, userId.Value);
                return Ok(new { message = "File deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST /api/files/{id}/restore
        [HttpPost("{id:int}/restore")]
        [Authorize]
        public async Task<IActionResult> Restore(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                await _fileService.RestoreFileAsync(id, userId.Value);
                return Ok(new { message = "File restored successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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