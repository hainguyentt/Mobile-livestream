using LivestreamApp.Profiles.Domain.Entities;

namespace LivestreamApp.Profiles.Services;

/// <summary>Manages user photo uploads via S3 presigned URLs.</summary>
public interface IPhotoService
{
    /// <summary>Generates a presigned S3 upload URL for a user photo.</summary>
    /// <param name="displayIndex">Display order index (0-5). Index 0 is the primary avatar.</param>
    /// <returns>Tuple of (presigned upload URL, new photo ID to use when confirming).</returns>
    /// <exception cref="DomainException">Thrown when displayIndex is invalid or user already has max photos.</exception>
    Task<(string UploadUrl, Guid PhotoId)> GeneratePresignedUploadUrlAsync(Guid userId, int displayIndex, string contentType, long fileSizeBytes, CancellationToken ct = default);

    /// <summary>Confirms a photo upload after the client has PUT the file to S3.</summary>
    /// <param name="displayIndex">Display index that was used when generating the presigned URL.</param>
    /// <exception cref="NotFoundException">Thrown when the S3 object is not found.</exception>
    Task<UserPhoto> ConfirmPhotoUploadAsync(Guid userId, Guid photoId, int displayIndex, string s3Key, string s3Url, long fileSizeBytes, string mimeType, CancellationToken ct = default);

    /// <summary>Deletes a photo and removes it from S3.</summary>
    /// <exception cref="NotFoundException">Thrown when the photo does not belong to the user.</exception>
    Task DeletePhotoAsync(Guid userId, Guid photoId, CancellationToken ct = default);

    /// <summary>Reorders photos by assigning new display indices.</summary>
    /// <param name="orderedPhotoIds">Photo IDs in the desired display order (index 0 first).</param>
    /// <exception cref="DomainException">Thrown when the provided IDs don't match the user's photos.</exception>
    Task ReorderPhotosAsync(Guid userId, Guid[] orderedPhotoIds, CancellationToken ct = default);
}
