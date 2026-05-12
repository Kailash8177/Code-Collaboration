using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CodeSync.Auth.Data;
using CodeSync.Auth.DTOs;
using CodeSync.Auth.Models;
using CodeSync.Auth.Repositories;
using CodeSync.Events.Users;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CodeSync.Auth.Services
{
    public class AuthServiceImpl : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly AuthDbContext _db;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<AuthServiceImpl> _logger;

        private string JwtSecret => _config["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured");
        private int JwtExpiryHours =>
            int.Parse(_config["Jwt:ExpiryHours"] ?? "24");
        private int RefreshExpiryDays =>
            int.Parse(_config["Jwt:RefreshExpiryDays"] ?? "7");

        public AuthServiceImpl(
            IUserRepository userRepo,
            AuthDbContext db,
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            IPublishEndpoint publishEndpoint,
            ILogger<AuthServiceImpl> logger)
        {
            _userRepo        = userRepo;
            _db              = db;
            _config          = config;
            _httpClient      = httpClientFactory.CreateClient();
            _publishEndpoint = publishEndpoint;
            _logger          = logger;
        }

        // ── Register ──────────────────────────────────────────────────────────

        public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
        {
            if (await _userRepo.ExistsByEmailAsync(req.Email))
                throw new InvalidOperationException("Email already registered.");

            if (await _userRepo.ExistsByUsernameAsync(req.Username))
                throw new InvalidOperationException("Username already taken.");

            var user = new User
            {
                Username     = req.Username,
                Email        = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                FullName     = req.FullName,
                Provider     = "LOCAL",
                Role         = "DEVELOPER"
            };

            await _userRepo.CreateAsync(user);

            // ✅ SAGA — publish UserRegistered event
            // Notification-Service listens → sends welcome email
            await _publishEndpoint.Publish(new UserRegistered(
                UserId:       user.UserId,
                Username:     user.Username,
                Email:        user.Email,
                FullName:     user.FullName,
                RegisteredAt: user.CreatedAt
            ));

            _logger.LogInformation(
                "UserRegistered event published for UserId={UserId}", user.UserId);

            return await BuildAuthResponseAsync(user);
        }

        // ── Login ─────────────────────────────────────────────────────────────

        public async Task<AuthResponse> LoginAsync(LoginRequest req)
        {
            var user = await _userRepo.FindByEmailAsync(req.Email)
                ?? throw new UnauthorizedAccessException("Invalid credentials.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated.");

            if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials.");

            return await BuildAuthResponseAsync(user);
        }

        // ── Logout ────────────────────────────────────────────────────────────

        public async Task LogoutAsync(string refreshToken)
        {
            var entry = await _db.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (entry is not null)
            {
                entry.IsRevoked = true;
                await _db.SaveChangesAsync();
            }
        }

        // ── Refresh ───────────────────────────────────────────────────────────

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            var entry = await _db.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken && !r.IsRevoked)
                ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            if (entry.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token has expired.");

            var user = await _userRepo.FindByUserIdAsync(entry.UserId)
                ?? throw new UnauthorizedAccessException("User not found.");

            entry.IsRevoked = true;
            await _db.SaveChangesAsync();

            return await BuildAuthResponseAsync(user);
        }

        // ── OAuth2 ────────────────────────────────────────────────────────────

        public async Task<AuthResponse> OAuthLoginAsync(OAuthLoginRequest req)
        {
            (string email, string name, string avatarUrl) = req.Provider.ToUpper() switch
            {
                "GITHUB" => await FetchGitHubProfileAsync(req.AccessToken),
                "GOOGLE" => await FetchGoogleProfileAsync(req.AccessToken),
                _        => throw new ArgumentException("Unsupported OAuth provider.")
            };

            var user      = await _userRepo.FindByEmailAsync(email);
            bool isNewUser = user is null;

            if (user is null)
            {
                var baseUsername = email.Split('@')[0];
                var username     = await EnsureUniqueUsernameAsync(baseUsername);

                user = new User
                {
                    Email        = email,
                    Username     = username,
                    FullName     = name,
                    AvatarUrl    = avatarUrl,
                    Provider     = req.Provider.ToUpper(),
                    PasswordHash = string.Empty,
                    Role         = "DEVELOPER"
                };

                await _userRepo.CreateAsync(user);
            }

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated.");

            // ✅ SAGA — publish UserRegistered only for brand new OAuth users
            if (isNewUser)
            {
                await _publishEndpoint.Publish(new UserRegistered(
                    UserId:       user.UserId,
                    Username:     user.Username,
                    Email:        user.Email,
                    FullName:     user.FullName,
                    RegisteredAt: user.CreatedAt
                ));

                _logger.LogInformation(
                    "UserRegistered event published for OAuth UserId={UserId}", user.UserId);
            }

            return await BuildAuthResponseAsync(user);
        }

        // ── Token validation ──────────────────────────────────────────────────

        public Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key     = Encoding.UTF8.GetBytes(JwtSecret);

                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(key),
                    ValidateIssuer           = false,
                    ValidateAudience         = false,
                    ClockSkew                = TimeSpan.Zero
                }, out _);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public async Task<User?> GetUserFromTokenAsync(string token)
        {
            try
            {
                var handler     = new JwtSecurityTokenHandler();
                var jwt         = handler.ReadJwtToken(token);
                var userIdClaim = jwt.Claims
                    .FirstOrDefault(c => c.Type == "userId")?.Value;

                if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
                    return null;

                return await _userRepo.FindByUserIdAsync(userId);
            }
            catch { return null; }
        }

        // ── Profile ───────────────────────────────────────────────────────────

        public async Task<UserProfileResponse> GetUserByIdAsync(int userId)
        {
            var user = await _userRepo.FindByUserIdAsync(userId)
                ?? throw new KeyNotFoundException($"User {userId} not found.");
            return MapToProfile(user);
        }

        public async Task<UserProfileResponse> GetUserByEmailAsync(string email)
        {
            var user = await _userRepo.FindByEmailAsync(email)
                ?? throw new KeyNotFoundException($"User {email} not found.");
            return MapToProfile(user);
        }

        public async Task<UserProfileResponse> UpdateProfileAsync(
            int userId, UpdateProfileRequest req)
        {
            var user = await _userRepo.FindByUserIdAsync(userId)
                ?? throw new KeyNotFoundException($"User {userId} not found.");

            if (req.Username is not null && req.Username != user.Username)
            {
                if (await _userRepo.ExistsByUsernameAsync(req.Username))
                    throw new InvalidOperationException("Username already taken.");
                user.Username = req.Username;
            }

            if (req.FullName  is not null) user.FullName  = req.FullName;
            if (req.AvatarUrl is not null) user.AvatarUrl = req.AvatarUrl;
            if (req.Bio       is not null) user.Bio       = req.Bio;

            await _userRepo.UpdateAsync(user);
            return MapToProfile(user);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordRequest req)
        {
            var user = await _userRepo.FindByUserIdAsync(userId)
                ?? throw new KeyNotFoundException($"User {userId} not found.");

            if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
            await _userRepo.UpdateAsync(user);
        }

        // ── Discovery ─────────────────────────────────────────────────────────

        public async Task<IEnumerable<UserProfileResponse>> SearchUsersAsync(string query)
        {
            var users = await _userRepo.SearchByUsernameAsync(query);
            return users.Select(MapToProfile);
        }

        public async Task<IEnumerable<UserProfileResponse>> GetAllByRoleAsync(string role)
        {
            var users = await _userRepo.FindAllByRoleAsync(role);
            return users.Select(MapToProfile);
        }

        // ── Admin ─────────────────────────────────────────────────────────────

        public async Task DeactivateAccountAsync(int userId)
        {
            var user = await _userRepo.FindByUserIdAsync(userId)
                ?? throw new KeyNotFoundException($"User {userId} not found.");

            user.IsActive = false;
            await _userRepo.UpdateAsync(user);

            // ✅ SAGA — publish UserDeactivated event
            // Project-Service listens → removes from all projects
            // Collab-Service listens  → kicks from all sessions
            // Auth-Service listens    → revokes all refresh tokens
            await _publishEndpoint.Publish(new UserDeactivated(
                UserId:        userId,
                DeactivatedAt: DateTime.UtcNow
            ));

            _logger.LogInformation(
                "UserDeactivated event published for UserId={UserId}", userId);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<AuthResponse> BuildAuthResponseAsync(User user)
        {
            var accessToken  = GenerateJwt(user);
            var refreshToken = await StoreRefreshTokenAsync(user.UserId);

            return new AuthResponse
            {
                Token        = accessToken,
                RefreshToken = refreshToken,
                UserId       = user.UserId,
                Username     = user.Username,
                Email        = user.Email,
                Role         = user.Role,
                AvatarUrl    = user.AvatarUrl
            };
        }

        private string GenerateJwt(User user)
        {
            var key   = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(JwtSecret));
            var creds = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId",         user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name,  user.Username),
                new Claim(ClaimTypes.Role,  user.Role)
            };

            var token = new JwtSecurityToken(
                issuer:             "CodeSync",
                audience:           "CodeSync",
                claims:             claims,
                expires:            DateTime.UtcNow.AddHours(JwtExpiryHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> StoreRefreshTokenAsync(int userId)
        {
            var old = _db.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked);
            await old.ForEachAsync(r => r.IsRevoked = true);

            var token = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));

            _db.RefreshTokens.Add(new RefreshTokenEntry
            {
                UserId    = userId,
                Token     = token,
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshExpiryDays)
            });

            await _db.SaveChangesAsync();
            return token;
        }

        private async Task<string> EnsureUniqueUsernameAsync(string base_)
        {
            var username = base_;
            var counter  = 1;
            while (await _userRepo.ExistsByUsernameAsync(username))
                username = $"{base_}{counter++}";
            return username;
        }

        private static UserProfileResponse MapToProfile(User u) => new()
        {
            UserId    = u.UserId,
            Username  = u.Username,
            Email     = u.Email,
            FullName  = u.FullName,
            Role      = u.Role,
            AvatarUrl = u.AvatarUrl,
            Provider  = u.Provider,
            Bio       = u.Bio,
            CreatedAt = u.CreatedAt,
            IsActive  = u.IsActive
        };

        // ── OAuth provider fetchers ───────────────────────────────────────────

        private async Task<(string email, string name, string avatarUrl)>
            FetchGitHubProfileAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "Authorization", $"Bearer {accessToken}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CodeSync");

            var profile = await _httpClient
                .GetFromJsonAsync<GitHubProfile>("https://api.github.com/user")
                ?? throw new Exception("Failed to fetch GitHub profile.");

            var email = profile.Email;
            if (string.IsNullOrEmpty(email))
            {
                var emails = await _httpClient
                    .GetFromJsonAsync<GitHubEmail[]>(
                        "https://api.github.com/user/emails");
                email = emails?
                    .FirstOrDefault(e => e.Primary && e.Verified)?.Email
                    ?? throw new Exception("No verified primary email on GitHub.");
            }

            return (email,
                    profile.Name ?? profile.Login,
                    profile.AvatarUrl ?? string.Empty);
        }

        private async Task<(string email, string name, string avatarUrl)>
            FetchGoogleProfileAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "Authorization", $"Bearer {accessToken}");

            var profile = await _httpClient
                .GetFromJsonAsync<GoogleProfile>(
                    "https://www.googleapis.com/oauth2/v3/userinfo")
                ?? throw new Exception("Failed to fetch Google profile.");

            return (profile.Email,
                    profile.Name ?? profile.Email,
                    profile.Picture ?? string.Empty);
        }

        private record GitHubProfile(
            string Login,
            string? Name,
            string? Email,
            string? AvatarUrl);

        private record GitHubEmail(
            string Email,
            bool Primary,
            bool Verified);

        private record GoogleProfile(
            string Email,
            string? Name,
            string? Picture);
    }
}