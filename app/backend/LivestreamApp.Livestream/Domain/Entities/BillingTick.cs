using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.Livestream.Domain.Entities;

/// <summary>Audit record for each coin deduction during a private call.</summary>
public sealed class BillingTick : Entity<Guid>
{
    public Guid CallSessionId { get; private set; }
    public int TickNumber { get; private set; }
    public int CoinsCharged { get; private set; }
    public int ViewerBalanceBefore { get; private set; }
    public int ViewerBalanceAfter { get; private set; }
    public DateTime ProcessedAt { get; private set; }
    public bool IsSuccess { get; private set; }

    private BillingTick(Guid id, Guid callSessionId, int tickNumber, int coinsCharged,
        int viewerBalanceBefore, int viewerBalanceAfter, bool isSuccess) : base(id)
    {
        CallSessionId = callSessionId;
        TickNumber = tickNumber;
        CoinsCharged = coinsCharged;
        ViewerBalanceBefore = viewerBalanceBefore;
        ViewerBalanceAfter = viewerBalanceAfter;
        IsSuccess = isSuccess;
        ProcessedAt = DateTime.UtcNow;
    }

    // EF Core constructor
    private BillingTick() : base(Guid.Empty) { }

    public static BillingTick Create(Guid callSessionId, int tickNumber, int coinsCharged,
        int viewerBalanceBefore, int viewerBalanceAfter, bool isSuccess)
    {
        if (callSessionId == Guid.Empty) throw new DomainException("CallSessionId is required.");
        if (tickNumber <= 0) throw new DomainException("TickNumber must be positive.");
        if (coinsCharged < 0) throw new DomainException("CoinsCharged cannot be negative.");

        return new BillingTick(Guid.NewGuid(), callSessionId, tickNumber, coinsCharged,
            viewerBalanceBefore, viewerBalanceAfter, isSuccess);
    }
}
