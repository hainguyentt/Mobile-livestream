using LivestreamApp.Auth.Domain.Entities;
using LivestreamApp.Shared.Domain.Enums;

namespace LivestreamApp.Auth.Repositories;

public interface IOtpRepository
{
    Task<OtpCode?> GetActiveAsync(string target, OtpPurpose purpose, CancellationToken ct = default);
    Task AddAsync(OtpCode otp, CancellationToken ct = default);
    Task InvalidatePreviousAsync(string target, OtpPurpose purpose, CancellationToken ct = default);
}
