using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.Livestream.Domain.Entities;

/// <summary>Ban record — viewer cannot rejoin the room until stream ends.</summary>
public sealed class KickedViewer : Entity<Guid>
{
    public Guid RoomId { get; private set; }
    public Guid ViewerId { get; private set; }
    public Guid KickedByUserId { get; private set; }
    public string KickedByRole { get; private set; } = string.Empty;
    public string? Reason { get; private set; }
    public DateTime KickedAt { get; private set; }

    private KickedViewer(Guid id, Guid roomId, Guid viewerId, Guid kickedByUserId, string kickedByRole, string? reason)
        : base(id)
    {
        RoomId = roomId;
        ViewerId = viewerId;
        KickedByUserId = kickedByUserId;
        KickedByRole = kickedByRole;
        Reason = reason;
        KickedAt = DateTime.UtcNow;
    }

    // EF Core constructor
    private KickedViewer() : base(Guid.Empty) { }

    public static KickedViewer Create(Guid roomId, Guid viewerId, Guid kickedByUserId, string kickedByRole, string? reason = null)
    {
        if (roomId == Guid.Empty) throw new DomainException("RoomId is required.");
        if (viewerId == Guid.Empty) throw new DomainException("ViewerId is required.");
        if (string.IsNullOrWhiteSpace(kickedByRole)) throw new DomainException("KickedByRole is required.");
        if (reason?.Length > 500) throw new DomainException("Reason must not exceed 500 characters.");

        return new KickedViewer(Guid.NewGuid(), roomId, viewerId, kickedByUserId, kickedByRole, reason);
    }
}
