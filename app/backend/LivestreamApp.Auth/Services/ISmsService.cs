namespace LivestreamApp.Auth.Services;

public interface ISmsService
{
    Task SendOtpAsync(string phoneNumber, string code, CancellationToken ct = default);
}
