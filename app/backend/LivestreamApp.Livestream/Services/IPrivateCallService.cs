using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Livestream.Domain.ValueObjects;

namespace LivestreamApp.Livestream.Services;

public interface IPrivateCallService
{
    Task<PrivateCallRequest> RequestCallAsync(Guid viewerId, Guid hostId, CancellationToken ct = default);
    Task<(CallSession Session, AgoraToken HostToken, AgoraToken ViewerToken)> AcceptCallAsync(Guid requestId, Guid hostId, CancellationToken ct = default);
    Task RejectCallAsync(Guid requestId, Guid hostId, string? reason = null, CancellationToken ct = default);
    Task EndCallAsync(Guid sessionId, Guid userId, string endedBy, CancellationToken ct = default);
    Task<AgoraToken> GetAgoraTokenAsync(Guid sessionId, Guid userId, CancellationToken ct = default);
    Task<CallSession?> GetCallStatusAsync(Guid sessionId, CancellationToken ct = default);
}
