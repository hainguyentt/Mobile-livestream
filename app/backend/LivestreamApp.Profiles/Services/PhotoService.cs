using LivestreamApp.Profiles.Domain.Entities;
using LivestreamApp.Profiles.Repositories;
using LivestreamApp.Shared.Exceptions;
using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Profiles.Services;

public class PhotoService : IPhotoService
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IStorageService _storage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public PhotoService(
        IPhotoRepository photoRepository,
        IStorageService storage,
        IUnitOfWork unitOfWork,
        ICacheService cache)
    {
        _photoRepository = photoRepository;
        _storage = storage;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    /// <inheritdoc/>
    public async Task<(string UploadUrl, Guid PhotoId)> GeneratePresignedUploadUrlAsync(
        Guid userId, int displayIndex, string contentType, long fileSizeBytes, CancellationToken ct = default)
    {
        if (displayIndex < 0 || displayIndex >= UserPhoto.MaxPhotosPerUser)
            throw new DomainException($"Display index must be between 0 and {UserPhoto.MaxPhotosPerUser - 1}.");

        var photoId = Guid.NewGuid();
        var s3Key = $"photos/{userId}/{photoId}";

        var uploadUrl = await _storage.GeneratePresignedUploadUrlAsync(s3Key, contentType, fileSizeBytes, ct);

        return (uploadUrl, photoId);
    }

    /// <inheritdoc/>
    public async Task<UserPhoto> ConfirmPhotoUploadAsync(
        Guid userId, Guid photoId, int displayIndex, string s3Key, string s3Url, long fileSizeBytes, string mimeType, CancellationToken ct = default)
    {
        var exists = await _storage.ObjectExistsAsync(s3Key, ct);
        if (!exists)
            throw new NotFoundException("S3 object", s3Key);

        // BR-PROFILE-02-3: if a photo already exists at this index, replace it
        var existingAtIndex = await _photoRepository.GetByUserIdAndIndexAsync(userId, displayIndex, ct);
        if (existingAtIndex is not null)
        {
            await _storage.DeleteObjectAsync(existingAtIndex.S3Key, ct);
            _photoRepository.Remove(existingAtIndex);
        }
        else
        {
            // Validate max photos limit
            var existingPhotos = await _photoRepository.GetByUserIdAsync(userId, ct);
            if (existingPhotos.Count >= UserPhoto.MaxPhotosPerUser)
                throw new DomainException($"Maximum of {UserPhoto.MaxPhotosPerUser} photos allowed.");
        }

        var photo = UserPhoto.Create(userId, s3Key, s3Url, displayIndex, fileSizeBytes, mimeType);
        await _photoRepository.AddAsync(photo, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Invalidate profile cache after photo change
        await _cache.RemoveAsync(CacheKeys.UserProfile(userId));

        return photo;
    }

    /// <inheritdoc/>
    public async Task DeletePhotoAsync(Guid userId, Guid photoId, CancellationToken ct = default)
    {
        var photo = await _photoRepository.GetByIdAndUserIdAsync(photoId, userId, ct)
            ?? throw new NotFoundException(nameof(UserPhoto), photoId);

        await _storage.DeleteObjectAsync(photo.S3Key, ct);
        _photoRepository.Remove(photo);
        await _unitOfWork.SaveChangesAsync(ct);

        // Invalidate profile cache after photo deletion
        await _cache.RemoveAsync(CacheKeys.UserProfile(userId));
    }

    /// <inheritdoc/>
    public async Task ReorderPhotosAsync(Guid userId, Guid[] orderedPhotoIds, CancellationToken ct = default)
    {
        var photos = await _photoRepository.GetByUserIdAsync(userId, ct);

        if (orderedPhotoIds.Length != photos.Count)
            throw new DomainException("Provided photo IDs do not match the user's photos.");

        var photoMap = photos.ToDictionary(p => p.Id);
        for (var i = 0; i < orderedPhotoIds.Length; i++)
        {
            if (!photoMap.TryGetValue(orderedPhotoIds[i], out var photo))
                throw new DomainException($"Photo {orderedPhotoIds[i]} does not belong to this user.");

            photo.UpdateDisplayIndex(i);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Invalidate profile cache after reorder
        await _cache.RemoveAsync(CacheKeys.UserProfile(userId));
    }
}
