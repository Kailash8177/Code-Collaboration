using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CodeSync.Gateway.Controllers;

/// <summary>
/// Lightweight controller exposing gateway metadata and a
/// live service-status dashboard. Swagger documents these endpoints.
/// YARP handles all /gateway/** proxy routes — these sit alongside it.
/// </summary>
[ApiController]
[Route("")]
public sealed class GatewayController : ControllerBase
{
    // ── Route Map ─────────────────────────────────────────────────────────
    private static readonly IReadOnlyDictionary<string, object> RouteMap =
        new Dictionary<string, object>
        {
            ["auth"]          = new { upstream = "/gateway/auth/**",          downstream = "http://localhost:7001/api/auth/**",          auth = "public" },
            ["projects"]      = new { upstream = "/gateway/projects/**",      downstream = "http://localhost:7002/api/projects/**",      auth = "JWT required" },
            ["files"]         = new { upstream = "/gateway/files/**",         downstream = "http://localhost:7003/api/files/**",         auth = "JWT required" },
            ["sessions"]      = new { upstream = "/gateway/sessions/**",      downstream = "http://localhost:7004/api/sessions/**",      auth = "JWT required" },
            ["executions"]    = new { upstream = "/gateway/executions/**",    downstream = "http://localhost:7005/api/executions/**",    auth = "JWT required" },
            ["snapshots"]     = new { upstream = "/gateway/snapshots/**",     downstream = "http://localhost:7006/api/snapshots/**",     auth = "JWT required" },
            ["comments"]      = new { upstream = "/gateway/comments/**",      downstream = "http://localhost:7006/api/comments/**",      auth = "JWT required" },
            ["notifications"] = new { upstream = "/gateway/notifications/**", downstream = "http://localhost:7007/api/notifications/**", auth = "JWT required" }
        };

    private readonly HealthCheckService _health;
    private readonly IConfiguration    _config;

    public GatewayController(HealthCheckService health, IConfiguration config)
    {
        _health = health;
        _config = config;
    }

    // ── GET / ─────────────────────────────────────────────────────────────
    /// <summary>Gateway welcome page with route map.</summary>
    [HttpGet("")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index()
    {
        return Ok(new
        {
            name        = "CodeSync API Gateway",
            engine      = "YARP — Yet Another Reverse Proxy",
            version     = "1.0",
            gatewayPort = 5000,
            swaggerUi   = "http://localhost:5000/swagger",
            healthCheck = "http://localhost:5000/health",
            routes      = RouteMap,
            timestamp   = DateTime.UtcNow
        });
    }

    // ── GET /ping ─────────────────────────────────────────────────────────
    /// <summary>Liveness probe — always returns pong.</summary>
    [HttpGet("ping")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping() =>
        Ok(new { message = "pong", gateway = "YARP", timestamp = DateTime.UtcNow });

    // ── GET /status ───────────────────────────────────────────────────────
    /// <summary>
    /// Runs active health checks against all 7 downstream services
    /// and returns a live status dashboard.
    /// </summary>
    [HttpGet("status")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Status()
    {
        var report = await _health.CheckHealthAsync();

        var services = report.Entries.Select(e => new
        {
            service     = e.Key,
            status      = e.Value.Status.ToString(),
            description = e.Value.Description,
            durationMs  = e.Value.Duration.TotalMilliseconds
        });

        var response = new
        {
            gateway   = "CodeSync API Gateway (YARP)",
            overall   = report.Status.ToString(),
            checkedAt = DateTime.UtcNow,
            services
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    // ── GET /routes ───────────────────────────────────────────────────────
    /// <summary>Returns the complete YARP route → cluster mapping.</summary>
    [HttpGet("routes")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Routes() =>
        Ok(new
        {
            description = "YARP route map — all routes proxy to their downstream cluster",
            baseUrl     = "http://localhost:5000",
            routes      = RouteMap
        });

    // ── GET /me  (authenticated) ──────────────────────────────────────────
    /// <summary>
    /// Returns the claims from the caller's JWT token.
    /// Use this to verify your token is valid and being parsed correctly.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(new
        {
            authenticated = true,
            userId        = User.FindFirst("sub")?.Value
                         ?? User.FindFirst("nameid")?.Value
                         ?? "unknown",
            username      = User.FindFirst("unique_name")?.Value ?? "unknown",
            role          = User.FindFirst("role")?.Value ?? "unknown",
            claims
        });
    }
}