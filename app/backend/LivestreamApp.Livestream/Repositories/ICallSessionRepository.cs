using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Shared.Domain.Enums;

namespace LivestreamApp.Livestream.Repositories;

public interface ICallSessionRepository
{
    Task<CallSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CallSession?> GetActiveSessionByHostAsync(Guid hostId, CancellationToken ct = default);
    Task<List<CallSession>> GetActiveSessionsAsync(CancellationToken ct = default);
    Task<PrivateCallRequest?> GetPendingRequestByHostAsync(Guid hostId, CancellationToken ct = default);
    Task<PrivateCallRequest?> GetRequestByIdAsync(Guid requestId, CancellationToken ct = default);
    Task AddRequestAsync(PrivateCallRequest request, CancellationToken ct = default);
    Task UpdateRequestAsync(PrivateCallRequest request, CancellationToken ct = default);
    Task AddSessionAsync(CallSession session, CancellationToken ct = default);
    Task UpdateSessionAsync(CallSession session, CancellationToken ct = default);
}
