using LivestreamApp.Auth.Services;

namespace LivestreamApp.API.Infrastructure.Services;

/// <summary>
/// Stub SMS service for local development — logs OTP codes to console.
/// Replace with real SMS provider (Twilio, AWS SNS) before production.
/// </summary>
public class StubSmsService : ISmsService
{
    private readonly ILogger<StubSmsService> _logger;

    public StubSmsService(ILogger<StubSmsService> logger) => _logger = logger;

    public Task SendOtpAsync(string phoneNumber, string code, CancellationToken ct = default)
    {
        // TODO: replace with real SMS provider (Twilio, AWS SNS) — Refs: NFR-U1-INFRA
        _logger.LogInformation("[STUB SMS] To: {Phone} | OTP: {Code}", phoneNumber, code);
        return Task.CompletedTask;
    }
}
