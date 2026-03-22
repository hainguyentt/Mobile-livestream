using LivestreamApp.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace LivestreamApp.API.Middleware;

/// <summary>Global exception handler — maps domain exceptions to RFC 7807 ProblemDetails HTTP responses.</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, type, title) = exception switch
        {
            DomainException       => (HttpStatusCode.BadRequest, "domain-error", "Domain Rule Violation"),
            NotFoundException     => (HttpStatusCode.NotFound, "not-found", "Resource Not Found"),
            UnauthorizedException => (HttpStatusCode.Unauthorized, "unauthorized", "Unauthorized"),
            ValidationException   => (HttpStatusCode.UnprocessableEntity, "validation-error", "Validation Failed"),
            _                     => (HttpStatusCode.InternalServerError, "internal-error", "An unexpected error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var problem = new ProblemDetails
        {
            Type = $"https://livestreamapp.io/errors/{type}",
            Title = title,
            Status = (int)statusCode,
            Detail = statusCode == HttpStatusCode.InternalServerError
                ? "An unexpected error occurred. Please try again later."
                : exception.Message,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
