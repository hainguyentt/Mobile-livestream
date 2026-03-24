using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Domain.Events;
using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.Livestream.Domain.Entities;

/// <summary>Viewer's request to start a private video call with a Host.</summary>
public sealed class PrivateCallRequest : Entity<Guid>
{
    public Guid ViewerId { get; private set; }
    public Guid HostId { get; private set; }
    public CallRequestStatus Status { get; private set; }
    public int CoinRatePerTick { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private PrivateCallRequest(Guid id, Guid viewerId, Guid hostId, int coinRatePerTick) : base(id)
    {
        ViewerId = viewerId;
        HostId = hostId;
        CoinRatePerTick = coinRatePerTick;
        Status = CallRequestStatus.Pending;
        RequestedAt = DateTime.UtcNow;
        ExpiresAt = RequestedAt.AddSeconds(30);
    }

    // EF Core constructor
    private PrivateCallRequest() : base(Guid.Empty) { }

    public static PrivateCallRequest Create(Guid viewerId, Guid hostId, int coinRatePerTick)
    {
        if (viewerId == Guid.Empty) throw new DomainException("ViewerId is required.");
        if (hostId == Guid.Empty) throw new DomainException("HostId is required.");
        if (coinRatePerTick <= 0) throw new DomainException("CoinRatePerTick must be positive.");

        return new PrivateCallRequest(Guid.NewGuid(), viewerId, hostId, coinRatePerTick);
    }

    public void Accept()
    {
        if (Status != CallRequestStatus.Pending)
            throw new DomainException("Only pending requests can be accepted.");
        if (DateTime.UtcNow > ExpiresAt)
            throw new DomainException("Request has expired.");

        Status = CallRequestStatus.Accepted;
        RespondedAt = DateTime.UtcNow;
        RaiseDomainEvent(new CallAcceptedEvent(Id, Guid.Empty, HostId, ViewerId));
    }

    public void Reject(string? reason = null)
    {
        if (Status != CallRequestStatus.Pending)
            throw new DomainException("Only pending requests can be rejected.");

        Status = CallRequestStatus.Rejected;
        RespondedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != CallRequestStatus.Pending)
            throw new DomainException("Only pending requests can be cancelled.");

        Status = CallRequestStatus.Cancelled;
        RespondedAt = DateTime.UtcNow;
    }

    public void MarkTimedOut()
    {
        if (Status != CallRequestStatus.Pending)
            throw new DomainException("Only pending requests can time out.");

        Status = CallRequestStatus.TimedOut;
        RespondedAt = DateTime.UtcNow;
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
}
