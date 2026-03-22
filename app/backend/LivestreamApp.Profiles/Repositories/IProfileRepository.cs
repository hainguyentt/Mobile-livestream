using LivestreamApp.Profiles.Domain.Entities;
using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Profiles.Repositories;

/// <summary>Repository for UserProfile aggregate.</summary>
public interface IProfileRepository : IRepository<UserProfile, Guid>
{
    /// <summary>Returns the profile for the given user, or null if not found.</summary>
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns true if the display name is already taken (case-insensitive).</summary>
    Task<bool> IsDisplayNameTakenAsync(string displayName, CancellationToken ct = default);

    /// <summary>Returns the profile with its photos eagerly loaded.</summary>
    Task<UserProfile?> GetWithPhotosAsync(Guid userId, CancellationToken ct = default);
}
