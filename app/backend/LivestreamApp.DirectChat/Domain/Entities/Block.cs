using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.DirectChat.Domain.Entities;

/// <summary>Block relationship — prevents messaging and hides conversation from both parties.</summary>
public sealed class Block : Entity<Guid>
{
    public Guid BlockerId { get; private set; }
    public Guid BlockedId { get; private set; }
    public DateTime BlockedAt { get; private set; }

    private Block(Guid id, Guid blockerId, Guid blockedId) : base(id)
    {
        BlockerId = blockerId;
        BlockedId = blockedId;
        BlockedAt = DateTime.UtcNow;
    }

    // EF Core constructor
    private Block() : base(Guid.Empty) { }

    public static Block Create(Guid blockerId, Guid blockedId)
    {
        if (blockerId == Guid.Empty) throw new DomainException("BlockerId is required.");
        if (blockedId == Guid.Empty) throw new DomainException("BlockedId is required.");
        if (blockerId == blockedId) throw new DomainException("Cannot block yourself.");
        return new Block(Guid.NewGuid(), blockerId, blockedId);
    }
}
