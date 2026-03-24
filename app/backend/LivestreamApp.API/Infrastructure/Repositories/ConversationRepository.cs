using LivestreamApp.DirectChat.Domain.Entities;
using LivestreamApp.DirectChat.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure.Repositories;

public sealed class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _db;
    public ConversationRepository(AppDbContext db) => _db = db;

    public Task<Conversation?> GetByIdForUserAsync(Guid conversationId, Guid userId, CancellationToken ct = default)
        => _db.Conversations.FirstOrDefaultAsync(
            c => c.Id == conversationId && (c.ViewerId == userId || c.HostId == userId), ct);

    public Task<Conversation?> GetByParticipantsAsync(Guid viewerId, Guid hostId, CancellationToken ct = default)
        => _db.Conversations.FirstOrDefaultAsync(
            c => c.ViewerId == viewerId && c.HostId == hostId, ct);

    public Task<List<Conversation>> GetVisibleByUserAsync(Guid userId, CancellationToken ct = default)
        => _db.Conversations
            .Where(c => (c.ViewerId == userId && !c.IsHiddenByViewer)
                     || (c.HostId == userId && !c.IsHiddenByHost))
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync(ct);

    public async Task AddAsync(Conversation conversation, CancellationToken ct = default)
        => await _db.Conversations.AddAsync(conversation, ct);

    public Task UpdateAsync(Conversation conversation, CancellationToken ct = default)
    {
        _db.Conversations.Update(conversation);
        return Task.CompletedTask;
    }
}
