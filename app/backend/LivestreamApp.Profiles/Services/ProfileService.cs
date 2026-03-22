using LivestreamApp.Profiles.Domain.Entities;
using LivestreamApp.Profiles.Repositories;
using LivestreamApp.Shared.Exceptions;
using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Profiles.Services;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProfileService(IProfileRepository profileRepository, IUnitOfWork unitOfWork)
    {
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<UserProfile> CreateProfileAsync(Guid userId, string displayName, DateOnly dateOfBirth, CancellationToken ct = default)
    {
        var existing = await _profileRepository.GetByUserIdAsync(userId, ct);
        if (existing is not null)
            throw new DomainException("Profile already exists for this user.");

        if (await _profileRepository.IsDisplayNameTakenAsync(displayName, ct))
            throw new DomainException($"Display name '{displayName}' is already taken.");

        var profile = UserProfile.Create(userId, displayName, dateOfBirth);
        await _profileRepository.AddAsync(profile, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return profile;
    }

    /// <inheritdoc/>
    public async Task<UserProfile> UpdateProfileAsync(Guid userId, string? bio, string[]? interests, string? preferredLanguage, CancellationToken ct = default)
    {
        var profile = await _profileRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(nameof(UserProfile), userId);

        profile.Update(bio, interests, preferredLanguage);
        await _unitOfWork.SaveChangesAsync(ct);

        return profile;
    }

    /// <inheritdoc/>
    public async Task<UserProfile> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        // Note: we do NOT cache the domain entity directly — System.Text.Json cannot deserialize
        // entities with private constructors. Cache is skipped here; use a read model cache if needed.
        var profile = await _profileRepository.GetWithPhotosAsync(userId, ct)
            ?? throw new NotFoundException(nameof(UserProfile), userId);

        return profile;
    }

    /// <inheritdoc/>
    public Task InvalidateProfileCacheAsync(Guid userId, CancellationToken ct = default) =>
        Task.CompletedTask; // No-op: profile caching removed (entity has private constructors)
}
