namespace LivestreamApp.Livestream.Services;

public interface IViewerCountService
{
    Task<long> IncrementAsync(Guid roomId, CancellationToken ct = default);
    Task<long> DecrementAsync(Guid roomId, CancellationToken ct = default);
    Task<long> GetCountAsync(Guid roomId, CancellationToken ct = default);
    Task<Dictionary<Guid, long>> GetCountsAsync(IEnumerable<Guid> roomIds, CancellationToken ct = default);
    Task ResetAsync(Guid roomId, CancellationToken ct = default);
}
