using LivestreamApp.DirectChat.Domain.Entities;

namespace LivestreamApp.DirectChat.Repositories;

public interface IBlockRepository
{
    Task<bool> ExistsAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default);
    Task<bool> ExistsInEitherDirectionAsync(Guid userId1, Guid userId2, CancellationToken ct = default);
    Task AddAsync(Block block, CancellationToken ct = default);
    Task DeleteAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default);
}
