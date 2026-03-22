using LivestreamApp.Profiles.Domain.Entities;

namespace LivestreamApp.Profiles.Services;

/// <summary>Manages user profile creation and updates.</summary>
public interface IProfileService
{
    /// <summary>Creates a new profile for the given user.</summary>
    /// <exception cref="DomainException">Thrown when displayName is already taken or profile already exists.</exception>
    Task<UserProfile> CreateProfileAsync(Guid userId, string displayName, DateOnly dateOfBirth, CancellationToken ct = default);

    /// <summary>Updates bio, interests, and preferred language for the given user.</summary>
    /// <remarks>Invalidates the profile cache after a successful update.</remarks>
    /// <exception cref="NotFoundException">Thrown when the profile does not exist.</exception>
    Task<UserProfile> UpdateProfileAsync(Guid userId, string? bio, string[]? interests, string? preferredLanguage, CancellationToken ct = default);

    /// <summary>Returns the profile for the given user. Checks cache before hitting the database.</summary>
    /// <exception cref="NotFoundException">Thrown when the profile does not exist.</exception>
    Task<UserProfile> GetProfileAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Removes the cached profile for the given user.</summary>
    Task InvalidateProfileCacheAsync(Guid userId, CancellationToken ct = default);
}
