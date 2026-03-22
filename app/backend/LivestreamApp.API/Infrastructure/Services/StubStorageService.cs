using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.API.Infrastructure.Services;

/// <summary>
/// Stub storage service for local development — simulates S3 operations.
/// Replace with real AWS S3 / LocalStack implementation before production.
/// </summary>
public class StubStorageService : IStorageService
{
    private readonly ILogger<StubStorageService> _logger;

    // Simulate uploaded objects in memory for local testing
    private static readonly HashSet<string> _uploadedKeys = [];

    public StubStorageService(ILogger<StubStorageService> logger) => _logger = logger;

    public Task<string> GeneratePresignedUploadUrlAsync(string s3Key, string contentType, long maxFileSizeBytes, CancellationToken ct = default)
    {
        // TODO: replace with real S3 presigned URL generation — Refs: NFR-U1-INFRA
        // Stub pre-registers the key so ObjectExistsAsync returns true without a real upload.
        // The stub URL uses a special marker so the frontend can skip the actual PUT.
        _uploadedKeys.Add(s3Key);
        var stubUrl = $"http://localhost:5174/api/v1/profiles/photos/stub-upload/{Uri.EscapeDataString(s3Key)}";
        _logger.LogInformation("[STUB S3] Presigned URL for key: {Key}", s3Key);
        return Task.FromResult(stubUrl);
    }

    public Task<bool> ObjectExistsAsync(string s3Key, CancellationToken ct = default)
    {
        // TODO: replace with real S3 HeadObject call — Refs: NFR-U1-INFRA
        return Task.FromResult(_uploadedKeys.Contains(s3Key));
    }

    public Task DeleteObjectAsync(string s3Key, CancellationToken ct = default)
    {
        // TODO: replace with real S3 DeleteObject call — Refs: NFR-U1-INFRA
        _uploadedKeys.Remove(s3Key);
        _logger.LogInformation("[STUB S3] Deleted key: {Key}", s3Key);
        return Task.CompletedTask;
    }
}
