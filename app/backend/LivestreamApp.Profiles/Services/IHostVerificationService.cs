using LivestreamApp.Profiles.Domain.Entities;

namespace LivestreamApp.Profiles.Services;

/// <summary>Manages host verification badge requests and admin approvals.</summary>
public interface IHostVerificationService
{
    /// <summary>Submits a verification request for the given host user.</summary>
    /// <remarks>Creates a HostProfile if one does not already exist.</remarks>
    /// <exception cref="DomainException">Thrown when a pending or approved request already exists.</exception>
    Task<HostProfile> RequestVerificationAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Approves the verification request and grants the verified badge.</summary>
    /// <exception cref="NotFoundException">Thrown when the host profile does not exist.</exception>
    /// <exception cref="DomainException">Thrown when no pending request exists.</exception>
    Task<HostProfile> ApproveVerificationAsync(Guid userId, Guid adminId, CancellationToken ct = default);

    /// <summary>Rejects the verification request with an optional admin note.</summary>
    /// <exception cref="NotFoundException">Thrown when the host profile does not exist.</exception>
    /// <exception cref="DomainException">Thrown when no pending request exists.</exception>
    Task<HostProfile> RejectVerificationAsync(Guid userId, Guid adminId, string? note, CancellationToken ct = default);
}
