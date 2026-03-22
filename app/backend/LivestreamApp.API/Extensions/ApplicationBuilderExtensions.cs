using LivestreamApp.API.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

namespace LivestreamApp.API.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>Registers custom middleware pipeline in the correct order.</summary>
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();
        return app;
    }

    /// <summary>Maps health check endpoints: /health/live, /health/ready, /health/startup.</summary>
    public static IEndpointRouteBuilder MapHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        // Liveness — simple 200 OK, process is alive
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteHealthResponse
        });

        // Readiness — checks PostgreSQL + Redis
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthResponse
        });

        // Startup — all checks
        endpoints.MapHealthChecks("/health/startup", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthResponse
        });

        return endpoints;
    }

    private static Task WriteHealthResponse(HttpContext context, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString() })
        });
        return context.Response.WriteAsync(result);
    }
}
