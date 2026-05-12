using CodeSync.Auth.DTOs;
using CodeSync.Auth.Models;

namespace CodeSync.Auth.Services
{
    public interface IAuthService
    {
        // ── Authentication ────────────────────────────────────────────────────
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task LogoutAsync(string refreshToken);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task<AuthResponse> OAuthLoginAsync(OAuthLoginRequest request);

        // ── Token ─────────────────────────────────────────────────────────────
        Task<bool> ValidateTokenAsync(string token);
        Task<User?> GetUserFromTokenAsync(string token);

        // ── Profile ───────────────────────────────────────────────────────────
        Task<UserProfileResponse> GetUserByIdAsync(int userId);
        Task<UserProfileResponse> GetUserByEmailAsync(string email);
        Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task ChangePasswordAsync(int userId, ChangePasswordRequest request);

        // ── Discovery ─────────────────────────────────────────────────────────
        Task<IEnumerable<UserProfileResponse>> SearchUsersAsync(string query);
        Task<IEnumerable<UserProfileResponse>> GetAllByRoleAsync(string role);

        // ── Admin ─────────────────────────────────────────────────────────────
        Task DeactivateAccountAsync(int userId);
    }
}