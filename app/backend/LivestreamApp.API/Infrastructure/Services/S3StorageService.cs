using Amazon.S3;
using Amazon.S3.Model;
using LivestreamApp.API.Infrastructure.Options;
using LivestreamApp.Shared.Interfaces;
using Microsoft.Extensions.Options;

namespace LivestreamApp.API.Infrastructure.Services;

/// <summary>
/// S3 storage service backed by AWS S3 or LocalStack.
/// Configured via S3Options — set ServiceUrl to LocalStack endpoint in development.
/// </summary>
public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly S3Options _options;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(IAmazonS3 s3, IOptions<S3Options> options, ILogger<S3StorageService> logger)
    {
        _s3 = s3;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> GeneratePresignedUploadUrlAsync(
        string s3Key, string contentType, long maxFileSizeBytes, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = s3Key,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = DateTime.UtcNow.AddMinutes(_options.PresignExpiryMinutes),
        };

        var url = await _s3.GetPreSignedURLAsync(request);

        // LocalStack returns https:// even when ServiceURL is http:// — normalize to http for local dev
        if (!string.IsNullOrEmpty(_options.ServiceUrl) && _options.ServiceUrl.StartsWith("http://"))
            url = url.Replace("https://localhost:4566", "http://localhost:4566");

        _logger.LogInformation("[S3] Generated presigned URL for key: {Key}", s3Key);
        return url;
    }

    /// <inheritdoc/>
    public async Task<bool> ObjectExistsAsync(string s3Key, CancellationToken ct = default)
    {
        try
        {
            await _s3.GetObjectMetadataAsync(_options.BucketName, s3Key, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteObjectAsync(string s3Key, CancellationToken ct = default)
    {
        await _s3.DeleteObjectAsync(_options.BucketName, s3Key, ct);
        _logger.LogInformation("[S3] Deleted key: {Key}", s3Key);
    }
}
