using LivestreamApp.Livestream.Domain.Entities;

namespace LivestreamApp.Livestream.Repositories;

public interface IViewerSessionRepository
{
    Task<ViewerSession?> GetActiveSessionAsync(Guid roomId, Guid viewerId, CancellationToken ct = default);
    Task<int> CountActiveViewersAsync(Guid roomId, CancellationToken ct = default);
    Task<bool> IsViewerKickedAsync(Guid roomId, Guid viewerId, CancellationToken ct = default);
    Task AddAsync(ViewerSession session, CancellationToken ct = default);
    Task UpdateAsync(ViewerSession session, CancellationToken ct = default);
}
