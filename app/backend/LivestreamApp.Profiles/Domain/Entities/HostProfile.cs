using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Events;
using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.Profiles.Domain.Entities;

/// <summary>Represents a host's verification status and statistics. Created when a user requests host verification.</summary>
public sealed class HostProfile : Entity<Guid>
{
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public Guid? VerifiedByAdminId { get; private set; }
    public DateTime? VerificationRequestedAt { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; }
    public string? VerificationNote { get; private set; }
    public long TotalCoinsReceived { get; private set; }
    public int TotalGiftsReceived { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private HostProfile(Guid userId) : base(userId)
    {
        VerificationStatus = VerificationStatus.None;
        CreatedAt = DateTime.UtcNow;
    }

    // EF Core constructor
    private HostProfile() : base(Guid.Empty) { }

    /// <summary>Creates a HostProfile for a user. UserId is used as the entity ID (1-1 with User).</summary>
    public static HostProfile Create(Guid userId) => new(userId);

    /// <summary>Submits a verification request. Only allowed when status is None or Rejected.</summary>
    /// <exception cref="DomainException">Thrown when a pending or approved request already exists.</exception>
    public void RequestVerification()
    {
        if (VerificationStatus is VerificationStatus.Pending or VerificationStatus.Approved)
            throw new DomainException("Verification request already exists.");

        VerificationStatus = VerificationStatus.Pending;
        VerificationRequestedAt = DateTime.UtcNow;
        RaiseDomainEvent(new HostVerificationRequestedEvent(Id, DateTime.UtcNow));
    }

    /// <summary>Approves the verification request and grants the verified badge.</summary>
    /// <exception cref="DomainException">Thrown when no pending request exists.</exception>
    public void Approve(Guid adminId)
    {
        if (VerificationStatus != VerificationStatus.Pending)
            throw new DomainException("No pending verification request to approve.");

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        VerifiedByAdminId = adminId;
        VerificationStatus = VerificationStatus.Approved;
        VerificationNote = null;
        RaiseDomainEvent(new HostVerifiedEvent(Id, adminId, DateTime.UtcNow));
    }

    /// <summary>Rejects the verification request with an optional note.</summary>
    /// <exception cref="DomainException">Thrown when no pending request exists.</exception>
    public void Reject(Guid adminId, string? note)
    {
        if (VerificationStatus != VerificationStatus.Pending)
            throw new DomainException("No pending verification request to reject.");

        VerificationStatus = VerificationStatus.Rejected;
        VerifiedByAdminId = adminId;
        VerificationNote = note;
    }
}
