using LivestreamApp.DirectChat.Domain.Entities;

namespace LivestreamApp.DirectChat.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdForUserAsync(Guid conversationId, Guid userId, CancellationToken ct = default);
    Task<Conversation?> GetByParticipantsAsync(Guid viewerId, Guid hostId, CancellationToken ct = default);
    Task<List<Conversation>> GetVisibleByUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Conversation conversation, CancellationToken ct = default);
    Task UpdateAsync(Conversation conversation, CancellationToken ct = default);
}
