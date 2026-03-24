using LivestreamApp.DirectChat.Domain.Entities;

namespace LivestreamApp.DirectChat.Repositories;

public interface IDirectMessageRepository
{
    /// <summary>
    /// Gets messages in a conversation. 'from' is required — partition safeguard.
    /// </summary>
    Task<List<DirectMessage>> GetMessagesAsync(Guid conversationId, DateTime from, DateTime? to = null, CancellationToken ct = default);
    Task AddAsync(DirectMessage message, CancellationToken ct = default);
    Task UpdateAsync(DirectMessage message, CancellationToken ct = default);
}
