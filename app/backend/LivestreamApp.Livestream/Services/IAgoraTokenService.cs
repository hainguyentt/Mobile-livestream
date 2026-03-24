using LivestreamApp.Livestream.Domain.ValueObjects;

namespace LivestreamApp.Livestream.Services;

public interface IAgoraTokenService
{
    Task<AgoraToken> GenerateTokenAsync(string channelName, Guid userId, AgoraRole role, CancellationToken ct = default);
    Task<long> GetCurrentMonthUsageMinutesAsync(CancellationToken ct = default);
    Task RevokeChannelAsync(string channelName, CancellationToken ct = default);
}
