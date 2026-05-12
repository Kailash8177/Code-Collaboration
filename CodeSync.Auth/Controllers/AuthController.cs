using CodeSync.Auth.DTOs;
using CodeSync.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSync.Auth.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // POST /api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // POST /api/auth/logout
        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return Ok(new { message = "Logged out successfully." });
        }

        // POST /api/auth/refresh
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request.RefreshToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // POST /api/auth/oauth
        [HttpPost("oauth")]
        [AllowAnonymous]
        public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequest request)
        {
            try
            {
                var result = await _authService.OAuthLoginAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/auth/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _authService.GetUserByIdAsync(userId.Value);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT /api/auth/profile
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                var result = await _authService
                    .UpdateProfileAsync(userId.Value, request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // PUT /api/auth/password
        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();
            try
            {
                await _authService.ChangePasswordAsync(userId.Value, request);
                return Ok(new { message = "Password changed successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // GET /api/auth/search?q=
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { message = "Search query is required." });

            var result = await _authService.SearchUsersAsync(q);
            return Ok(result);
        }

        // GET /api/auth/users/{id}
        [HttpGet("users/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var result = await _authService.GetUserByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST /api/auth/deactivate/{id}
        [HttpPost("deactivate/{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _authService.DeactivateAccountAsync(id);
                return Ok(new { message = $"User {id} deactivated." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET /api/auth/validate
        [HttpGet("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateToken(
            [FromHeader(Name = "Authorization")] string? authHeader)
        {
            if (string.IsNullOrEmpty(authHeader) ||
                !authHeader.StartsWith("Bearer "))
                return Unauthorized(new { valid = false });

            var token = authHeader["Bearer ".Length..];
            var valid = await _authService.ValidateTokenAsync(token);
            return Ok(new { valid });
        }

        // GET /api/auth/roles/{role}
        [HttpGet("roles/{role}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetByRole(string role)
        {
            var result = await _authService.GetAllByRoleAsync(role.ToUpper());
            return Ok(result);
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private int? GetCurrentUserId()
        {
            var claim = User.Claims
                .FirstOrDefault(c => c.Type == "userId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}