namespace LivestreamApp.Livestream.Domain.ValueObjects;

public enum AgoraRole { Publisher, Subscriber }

/// <summary>Agora RTC token with channel info and expiry.</summary>
public sealed record AgoraToken(
    string Token,
    string ChannelName,
    AgoraRole Role,
    DateTime ExpiresAt)
{
    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;
}
