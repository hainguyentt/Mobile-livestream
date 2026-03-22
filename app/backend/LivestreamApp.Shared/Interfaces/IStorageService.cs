namespace LivestreamApp.Shared.Interfaces;

/// <summary>Abstraction over S3-compatible object storage (AWS S3 / LocalStack).</summary>
public interface IStorageService
{
    /// <summary>Generates a presigned URL for a client to PUT an object directly to S3.</summary>
    /// <param name="s3Key">The target object key in the bucket.</param>
    /// <param name="contentType">Expected MIME type of the upload.</param>
    /// <param name="maxFileSizeBytes">Maximum allowed file size enforced by the presigned policy.</param>
    /// <returns>A time-limited presigned upload URL.</returns>
    Task<string> GeneratePresignedUploadUrlAsync(string s3Key, string contentType, long maxFileSizeBytes, CancellationToken ct = default);

    /// <summary>Returns true if the object exists in the bucket.</summary>
    Task<bool> ObjectExistsAsync(string s3Key, CancellationToken ct = default);

    /// <summary>Deletes an object from the bucket.</summary>
    Task DeleteObjectAsync(string s3Key, CancellationToken ct = default);
}
