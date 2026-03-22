using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;

namespace LivestreamApp.API.Middleware;

/// <summary>
/// Per-IP fixed window rate limiter.
/// Uses in-memory storage — suitable for single-instance dev/staging.
/// For multi-instance production, replace with Redis-backed implementation.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    // Key: "ip:{ip}:{endpoint_group}" → (count, windowStart)
    private static readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)> _counters = new();

    private static readonly Dictionary<string, (int Limit, int WindowSeconds)> _limits = new()
    {
        ["auth"]    = (20, 60),   // 20 req/min for auth endpoints
        ["otp"]     = (5, 60),    // 5 req/min for OTP endpoints
        ["default"] = (100, 60),  // 100 req/min for everything else
    };

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Disable rate limiting in test environment to allow integration tests to run freely
        if (_env.IsEnvironment("Testing"))
        {
            await _next(context);
            return;
        }

        var clientIp = GetClientIp(context);
        var group = GetEndpointGroup(context.Request.Path);
        var key = $"ip:{clientIp}:{group}";
        var (limit, windowSeconds) = _limits.GetValueOrDefault(group, _limits["default"]);

        var now = DateTime.UtcNow;
        var entry = _counters.GetOrAdd(key, _ => (0, now));

        // Reset window if expired
        if ((now - entry.WindowStart).TotalSeconds >= windowSeconds)
            entry = (0, now);

        entry = (entry.Count + 1, entry.WindowStart);
        _counters[key] = entry;

        if (entry.Count > limit)
        {
            _logger.LogWarning("Rate limit exceeded for IP {Ip} on group {Group}", clientIp, group);
            context.Response.StatusCode = 429;
            context.Response.Headers["Retry-After"] = windowSeconds.ToString();
            context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "rate_limit_exceeded",
                message = $"Too many requests. Please try again in {windowSeconds} seconds.",
                retryAfter = windowSeconds
            });
            return;
        }

        context.Response.Headers["X-RateLimit-Limit"] = limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = (limit - entry.Count).ToString();

        await _next(context);
    }

    private static string GetClientIp(HttpContext context) =>
        context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').First().Trim()
        ?? context.Connection.RemoteIpAddress?.ToString()
        ?? "unknown";

    private static string GetEndpointGroup(PathString path)
    {
        var p = path.Value?.ToLowerInvariant() ?? "";
        if (p.Contains("/otp/")) return "otp";
        if (p.Contains("/auth/")) return "auth";
        return "default";
    }
}
