using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Domain.Events;
using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.Livestream.Domain.Entities;

/// <summary>Aggregate root for a livestream room session.</summary>
public sealed class LivestreamRoom : Entity<Guid>
{
    public Guid HostId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public RoomCategory Category { get; private set; }
    public RoomStatus Status { get; private set; }
    public string AgoraChannelName { get; private set; } = string.Empty;
    public int ViewerCount { get; private set; }
    public int PeakViewerCount { get; private set; }
    public int TotalViewerCount { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public ICollection<ViewerSession> ViewerSessions { get; private set; } = [];
    public ICollection<KickedViewer> KickedViewers { get; private set; } = [];

    private LivestreamRoom(Guid id, Guid hostId, string title, RoomCategory category) : base(id)
    {
        HostId = hostId;
        Title = title;
        Category = category;
        Status = RoomStatus.Scheduled;
        AgoraChannelName = $"room-{id:N}";
        CreatedAt = DateTime.UtcNow;
    }

    // EF Core constructor
    private LivestreamRoom() : base(Guid.Empty) { }

    /// <summary>Creates a new livestream room. Host must not have an active stream.</summary>
    public static LivestreamRoom Create(Guid hostId, string title, RoomCategory category)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Room title is required.");
        if (title.Length > 100)
            throw new DomainException("Room title must not exceed 100 characters.");

        return new LivestreamRoom(Guid.NewGuid(), hostId, title, category);
    }

    /// <summary>Transitions room to Live status.</summary>
    public void StartStream()
    {
        if (Status != RoomStatus.Scheduled)
            throw new DomainException("Only scheduled rooms can be started.");

        Status = RoomStatus.Live;
        StartedAt = DateTime.UtcNow;
        RaiseDomainEvent(new StreamStartedEvent(Id, HostId, Title, Category, StartedAt.Value));
    }

    /// <summary>Ends the stream and records final stats.</summary>
    public void EndStream()
    {
        if (Status != RoomStatus.Live)
            throw new DomainException("Only live rooms can be ended.");

        Status = RoomStatus.Ended;
        EndedAt = DateTime.UtcNow;
        RaiseDomainEvent(new StreamEndedEvent(Id, HostId, EndedAt.Value, TotalViewerCount, PeakViewerCount));
    }

    /// <summary>Updates cached viewer count. Called by ViewerCountService.</summary>
    public void UpdateViewerCount(int count)
    {
        ViewerCount = Math.Max(0, count);
        if (ViewerCount > PeakViewerCount)
            PeakViewerCount = ViewerCount;
    }

    /// <summary>Increments total unique viewer count when a new viewer joins.</summary>
    public void IncrementTotalViewers() => TotalViewerCount++;
}
