using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Shared.Domain.Enums;

namespace LivestreamApp.Livestream.Services;

public interface ILivestreamService
{
    Task<LivestreamRoom> CreateRoomAsync(Guid hostId, string title, RoomCategory category, CancellationToken ct = default);
    Task StartStreamAsync(Guid roomId, Guid hostId, CancellationToken ct = default);
    Task EndStreamAsync(Guid roomId, Guid hostId, CancellationToken ct = default);
    Task<(LivestreamRoom Room, ViewerSession Session)> JoinRoomAsync(Guid roomId, Guid viewerId, CancellationToken ct = default);
    Task LeaveRoomAsync(Guid roomId, Guid viewerId, CancellationToken ct = default);
    Task<(List<LivestreamRoom> Rooms, int Total)> GetActiveRoomsAsync(RoomCategory? category, int page, int pageSize, CancellationToken ct = default);
    Task<LivestreamRoom?> GetRoomByIdAsync(Guid roomId, CancellationToken ct = default);
    Task BanViewerAsync(Guid roomId, Guid hostId, Guid viewerId, string? reason, CancellationToken ct = default);
    Task<int> GetViewerCountAsync(Guid roomId, CancellationToken ct = default);
    Task<List<ViewerSession>> GetActiveViewersAsync(Guid roomId, CancellationToken ct = default);
}
