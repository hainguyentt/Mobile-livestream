using LivestreamApp.Shared.Domain.Enums;

namespace LivestreamApp.Auth.Services;

public interface IEmailService
{
    Task SendOtpAsync(string toEmail, string code, OtpPurpose purpose, CancellationToken ct = default);
}
