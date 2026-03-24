using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.DirectChat.Domain.Entities;

/// <summary>1-1 conversation between a Viewer and a Host.</summary>
public sealed class Conversation : Entity<Guid>
{
    public Guid ViewerId { get; private set; }
    public Guid HostId { get; private set; }
    public DateTime? LastMessageAt { get; private set; }
    public string? LastMessagePreview { get; private set; }
    public int ViewerUnreadCount { get; private set; }
    public int HostUnreadCount { get; private set; }
    public bool IsHiddenByViewer { get; private set; }
    public bool IsHiddenByHost { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public ICollection<DirectMessage> Messages { get; private set; } = [];

    private Conversation(Guid id, Guid viewerId, Guid hostId) : base(id)
    {
        ViewerId = viewerId;
        HostId = hostId;
        CreatedAt = DateTime.UtcNow;
    }

    // EF Core constructor
    private Conversation() : base(Guid.Empty) { }

    public static Conversation Create(Guid viewerId, Guid hostId)
    {
        if (viewerId == Guid.Empty) throw new DomainException("ViewerId is required.");
        if (hostId == Guid.Empty) throw new DomainException("HostId is required.");
        if (viewerId == hostId) throw new DomainException("Viewer and Host cannot be the same user.");
        return new Conversation(Guid.NewGuid(), viewerId, hostId);
    }

    /// <summary>Updates conversation metadata after a new message is sent.</summary>
    public void RecordMessage(string preview, Guid senderId)
    {
        LastMessageAt = DateTime.UtcNow;
        LastMessagePreview = preview.Length > 100 ? preview[..100] : preview;

        if (senderId == ViewerId)
            HostUnreadCount++;
        else
            ViewerUnreadCount++;
    }

    public void MarkReadByViewer() => ViewerUnreadCount = 0;
    public void MarkReadByHost() => HostUnreadCount = 0;

    /// <summary>Hides conversation from both parties when a block occurs.</summary>
    public void HideForBoth()
    {
        IsHiddenByViewer = true;
        IsHiddenByHost = true;
    }

    public void UnhideForBoth()
    {
        IsHiddenByViewer = false;
        IsHiddenByHost = false;
    }
}
