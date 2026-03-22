namespace LivestreamApp.API.Infrastructure.Options;

/// <summary>Configuration for AWS S3 / LocalStack storage.</summary>
public class S3Options
{
    public const string SectionName = "S3";

    /// <summary>S3 bucket name for user photos.</summary>
    public string BucketName { get; set; } = "livestream-photos";

    /// <summary>AWS region (e.g. ap-northeast-1).</summary>
    public string Region { get; set; } = "ap-northeast-1";

    /// <summary>
    /// Override service URL for LocalStack in development.
    /// Leave empty for real AWS.
    /// </summary>
    public string? ServiceUrl { get; set; }

    /// <summary>
    /// Public base URL used to build the S3Url stored in DB.
    /// For LocalStack: http://localhost:4566/livestream-photos
    /// For real S3: https://livestream-photos.s3.ap-northeast-1.amazonaws.com
    /// </summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:4566/livestream-photos";

    /// <summary>Presigned URL expiry in minutes.</summary>
    public int PresignExpiryMinutes { get; set; } = 15;
}
