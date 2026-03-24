using LivestreamApp.DirectChat.Domain.Entities;
using LivestreamApp.DirectChat.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure.Repositories;

public sealed class DirectMessageRepository : IDirectMessageRepository
{
    private readonly AppDbContext _db;
    public DirectMessageRepository(AppDbContext db) => _db = db;

    public Task<List<DirectMessage>> GetMessagesAsync(
        Guid conversationId, DateTime from, DateTime? to = null, CancellationToken ct = default)
    {
        var query = _db.DirectMessages
            .Where(m => m.ConversationId == conversationId
                     && m.SentAt >= from
                     && !m.IsDeletedBySender);

        if (to.HasValue)
            query = query.Where(m => m.SentAt < to.Value);

        return query.OrderBy(m => m.SentAt).ToListAsync(ct);
    }

    public async Task AddAsync(DirectMessage message, CancellationToken ct = default)
        => await _db.DirectMessages.AddAsync(message, ct);

    public Task UpdateAsync(DirectMessage message, CancellationToken ct = default)
    {
        _db.DirectMessages.Update(message);
        return Task.CompletedTask;
    }
}
