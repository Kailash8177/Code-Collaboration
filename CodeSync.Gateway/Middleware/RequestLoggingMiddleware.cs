using System.Diagnostics;

namespace CodeSync.Gateway.Middleware;

/// <summary>
/// Logs every incoming request with method, path, status code,
/// elapsed time, and client IP. Helps debug proxy routing issues.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        // Capture client IP (respects X-Forwarded-For if behind another proxy)
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var fwd))
            clientIp = fwd.ToString().Split(',')[0].Trim();

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            var level = context.Response.StatusCode switch
            {
                >= 500 => LogLevel.Error,
                >= 400 => LogLevel.Warning,
                _      => LogLevel.Information
            };

            _logger.Log(
                level,
                "[GATEWAY] {Method} {Path}{Query} → {StatusCode} ({ElapsedMs}ms) | IP: {ClientIp}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                clientIp);
        }
    }
}