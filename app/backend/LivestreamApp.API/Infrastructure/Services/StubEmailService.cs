using LivestreamApp.Auth.Services;
using LivestreamApp.Shared.Domain.Enums;

namespace LivestreamApp.API.Infrastructure.Services;

/// <summary>
/// Stub email service for local development — logs OTP codes to console.
/// Replace with real SMTP/SES implementation before production.
/// </summary>
public class StubEmailService : IEmailService
{
    private readonly ILogger<StubEmailService> _logger;

    public StubEmailService(ILogger<StubEmailService> logger) => _logger = logger;

    public Task SendOtpAsync(string toEmail, string code, OtpPurpose purpose, CancellationToken ct = default)
    {
        // TODO: replace with real email provider (SES, SendGrid) — Refs: NFR-U1-INFRA
        _logger.LogInformation("[STUB EMAIL] To: {Email} | Purpose: {Purpose} | OTP: {Code}", toEmail, purpose, code);
        return Task.CompletedTask;
    }
}
