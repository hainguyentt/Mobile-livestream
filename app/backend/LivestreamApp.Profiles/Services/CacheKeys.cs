namespace LivestreamApp.Profiles.Services;

/// <summary>Centralized cache key definitions for the Profiles module.</summary>
public static class CacheKeys
{
    public static string UserProfile(Guid userId) => $"user:profile:{userId}";
}
