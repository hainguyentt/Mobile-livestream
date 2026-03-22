using LivestreamApp.Profiles.Domain.Entities;
using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Profiles.Repositories;

/// <summary>Repository for UserPhoto entities.</summary>
public interface IPhotoRepository : IRepository<UserPhoto, Guid>
{
    /// <summary>Returns all photos for a user ordered by DisplayIndex.</summary>
    Task<IReadOnlyList<UserPhoto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns a specific photo belonging to a user, or null if not found.</summary>
    Task<UserPhoto?> GetByIdAndUserIdAsync(Guid photoId, Guid userId, CancellationToken ct = default);

    /// <summary>Returns the count of photos for a user.</summary>
    Task<int> CountByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns the photo at a specific display index for a user, or null if none exists.</summary>
    Task<UserPhoto?> GetByUserIdAndIndexAsync(Guid userId, int displayIndex, CancellationToken ct = default);
}
