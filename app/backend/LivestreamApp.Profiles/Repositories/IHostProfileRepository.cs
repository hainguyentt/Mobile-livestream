using LivestreamApp.Profiles.Domain.Entities;
using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Profiles.Repositories;

/// <summary>Repository for HostProfile aggregate.</summary>
public interface IHostProfileRepository : IRepository<HostProfile, Guid>
{
    /// <summary>Returns the host profile for the given user, or null if not found.</summary>
    Task<HostProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
