using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Domain.Events;
using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.Livestream.Domain.Entities;

/// <summary>Active private video call session between a Viewer and a Host.</summary>
public sealed class CallSession : Entity<Guid>
{
    public Guid CallRequestId { get; private set; }
    public Guid ViewerId { get; private set; }
    public Guid HostId { get; private set; }
    public string AgoraChannelName { get; private set; } = string.Empty;
    public CallSessionStatus Status { get; private set; }
    public int CoinRatePerTick { get; private set; }
    public int TotalCoinsCharged { get; private set; }
    public int TotalTicks { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public string? EndedBy { get; private set; }

    // Navigation
    public ICollection<BillingTick> BillingTicks { get; private set; } = [];

    private CallSession(Guid id, Guid callRequestId, Guid viewerId, Guid hostId, int coinRatePerTick)
        : base(id)
    {
        CallRequestId = callRequestId;
        ViewerId = viewerId;
        HostId = hostId;
        CoinRatePerTick = coinRatePerTick;
        AgoraChannelName = $"call-{id:N}";
        Status = CallSessionStatus.Active;
        StartedAt = DateTime.UtcNow;
    }

    // EF Core constructor
    private CallSession() : base(Guid.Empty) { }

    public static CallSession Create(Guid callRequestId, Guid viewerId, Guid hostId, int coinRatePerTick)
    {
        if (callRequestId == Guid.Empty) throw new DomainException("CallRequestId is required.");
        if (viewerId == Guid.Empty) throw new DomainException("ViewerId is required.");
        if (hostId == Guid.Empty) throw new DomainException("HostId is required.");
        if (coinRatePerTick <= 0) throw new DomainException("CoinRatePerTick must be positive.");

        return new CallSession(Guid.NewGuid(), callRequestId, viewerId, hostId, coinRatePerTick);
    }

    /// <summary>Records a successful billing tick and updates totals.</summary>
    public void RecordBillingTick(int coinsCharged)
    {
        if (Status != CallSessionStatus.Active)
            throw new DomainException("Cannot bill an ended session.");

        TotalCoinsCharged += coinsCharged;
        TotalTicks++;
    }

    /// <summary>Ends the call session.</summary>
    /// <param name="endedBy">Viewer / Host / System</param>
    public void End(string endedBy)
    {
        if (Status != CallSessionStatus.Active)
            throw new DomainException("Session is already ended.");

        Status = CallSessionStatus.Ended;
        EndedAt = DateTime.UtcNow;
        EndedBy = endedBy;

        var durationSeconds = (int)(EndedAt.Value - StartedAt).TotalSeconds;
        RaiseDomainEvent(new CallEndedEvent(Id, endedBy, TotalCoinsCharged, durationSeconds));
    }
}
