using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.Livestream.Domain.Entities;

/// <summary>Tracks a viewer's join/leave session within a livestream room.</summary>
public sealed class ViewerSession : Entity<Guid>
{
    public Guid RoomId { get; private set; }
    public Guid ViewerId { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }
    public int WatchDurationSeconds { get; private set; }
    public bool IsKicked { get; private set; }

    private ViewerSession(Guid id, Guid roomId, Guid viewerId) : base(id)
    {
        RoomId = roomId;
        ViewerId = viewerId;
        JoinedAt = DateTime.UtcNow;
    }

    // EF Core constructor
    private ViewerSession() : base(Guid.Empty) { }

    public static ViewerSession Create(Guid roomId, Guid viewerId)
    {
        if (roomId == Guid.Empty) throw new DomainException("RoomId is required.");
        if (viewerId == Guid.Empty) throw new DomainException("ViewerId is required.");
        return new ViewerSession(Guid.NewGuid(), roomId, viewerId);
    }

    /// <summary>Records viewer leaving the room and calculates watch duration.</summary>
    public void RecordLeave()
    {
        if (LeftAt.HasValue) throw new DomainException("Session already ended.");
        LeftAt = DateTime.UtcNow;
        WatchDurationSeconds = (int)(LeftAt.Value - JoinedAt).TotalSeconds;
    }

    /// <summary>Marks session as kicked — viewer cannot rejoin until stream ends.</summary>
    public void MarkAsKicked()
    {
        IsKicked = true;
        RecordLeave();
    }
}
