using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Shared.Domain.Enums;

namespace LivestreamApp.Livestream.Repositories;

public interface ILivestreamRoomRepository
{
    Task<LivestreamRoom?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<LivestreamRoom?> GetActiveRoomByHostAsync(Guid hostId, CancellationToken ct = default);
    Task<List<LivestreamRoom>> GetActiveRoomsAsync(RoomCategory? category = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<int> CountActiveRoomsAsync(RoomCategory? category = null, CancellationToken ct = default);
    Task AddAsync(LivestreamRoom room, CancellationToken ct = default);
    Task UpdateAsync(LivestreamRoom room, CancellationToken ct = default);
}
