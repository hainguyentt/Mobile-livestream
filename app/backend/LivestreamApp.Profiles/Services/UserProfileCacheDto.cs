namespace LivestreamApp.Profiles.Services;

/// <summary>
/// Serialization-friendly DTO for caching UserProfile in Redis.
/// Uses public setters so System.Text.Json can deserialize it.
/// </summary>
public class UserProfileCacheDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public string[] Interests { get; set; } = [];
    public string PreferredLanguage { get; set; } = "ja";
    public bool IsProfileComplete { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<UserPhotoCacheDto> Photos { get; set; } = [];
}

/// <summary>Serialization-friendly DTO for caching UserPhoto in Redis.</summary>
public class UserPhotoCacheDto
{
    public Guid Id { get; set; }
    public int DisplayIndex { get; set; }
    public string S3Key { get; set; } = string.Empty;
    public string S3Url { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
