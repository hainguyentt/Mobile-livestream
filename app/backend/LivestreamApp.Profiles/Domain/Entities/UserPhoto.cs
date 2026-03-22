using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Events;
using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.Profiles.Domain.Entities;

/// <summary>Represents a user's uploaded photo. Max 6 photos per user (DisplayIndex 0-5).</summary>
public sealed class UserPhoto : Entity<Guid>
{
    public const int MaxPhotosPerUser = 6;

    public Guid UserId { get; private set; }
    public string S3Key { get; private set; }
    public string S3Url { get; private set; }
    public int DisplayIndex { get; private set; }
    public long FileSizeBytes { get; private set; }
    public string MimeType { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private UserPhoto(Guid id, Guid userId, string s3Key, string s3Url, int displayIndex, long fileSizeBytes, string mimeType)
        : base(id)
    {
        UserId = userId;
        S3Key = s3Key;
        S3Url = s3Url;
        DisplayIndex = displayIndex;
        FileSizeBytes = fileSizeBytes;
        MimeType = mimeType;
        UploadedAt = DateTime.UtcNow;
    }

    // EF Core constructor
    private UserPhoto() : base(Guid.Empty) { S3Key = null!; S3Url = null!; MimeType = null!; }

    /// <summary>Creates a confirmed photo record after successful S3 upload.</summary>
    /// <param name="displayIndex">Display order index (0-5). Index 0 is the primary avatar.</param>
    /// <exception cref="DomainException">Thrown when displayIndex is out of range or mimeType is unsupported.</exception>
    public static UserPhoto Create(Guid userId, string s3Key, string s3Url, int displayIndex, long fileSizeBytes, string mimeType)
    {
        if (displayIndex < 0 || displayIndex >= MaxPhotosPerUser)
            throw new DomainException($"Display index must be between 0 and {MaxPhotosPerUser - 1}.");

        if (!IsValidMimeType(mimeType))
            throw new DomainException($"Unsupported image type: {mimeType}. Allowed: image/jpeg, image/png, image/webp.");

        var photo = new UserPhoto(Guid.NewGuid(), userId, s3Key, s3Url, displayIndex, fileSizeBytes, mimeType);
        photo.RaiseDomainEvent(new PhotoUploadedEvent(userId, photo.Id, displayIndex, s3Key));
        return photo;
    }

    /// <summary>Updates the display index when photos are reordered.</summary>
    public void UpdateDisplayIndex(int newIndex)
    {
        if (newIndex < 0 || newIndex >= MaxPhotosPerUser)
            throw new DomainException($"Display index must be between 0 and {MaxPhotosPerUser - 1}.");

        DisplayIndex = newIndex;
    }

    private static bool IsValidMimeType(string mimeType) =>
        mimeType is "image/jpeg" or "image/png" or "image/webp";
}
