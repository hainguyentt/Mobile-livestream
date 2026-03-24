using LivestreamApp.Livestream.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LivestreamApp.Livestream.Services;

public sealed class AgoraTokenService : IAgoraTokenService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _config;
    private readonly ILogger<AgoraTokenService> _logger;

    private bool MockMode => bool.TryParse(_config["Agora:MockMode"], out var v) && v;
    private static string QuotaKey => "agora_quota:current_month";

    public AgoraTokenService(
        IConnectionMultiplexer redis,
        IConfiguration config,
        ILogger<AgoraTokenService> logger)
    {
        _redis = redis;
        _config = config;
        _logger = logger;
    }

    public Task<AgoraToken> GenerateTokenAsync(string channelName, Guid userId, AgoraRole role, CancellationToken ct = default)
    {
        if (MockMode)
        {
            _logger.LogDebug("Agora MockMode: returning fake token for channel {Channel}", channelName);
            var fakeToken = new AgoraToken(
                Token: $"mock-token-{channelName}-{userId:N}",
                ChannelName: channelName,
                Role: role,
                ExpiresAt: DateTime.UtcNow.AddHours(4));
            return Task.FromResult(fakeToken);
        }

        // Real Agora token generation
        // TODO: integrate Agora RTC Token Builder SDK
        // For now, generate a placeholder — replace with actual SDK call
        var appId = _config["Agora:AppId"] ?? throw new InvalidOperationException("Agora:AppId not configured.");
        var appCertificate = _config["Agora:AppCertificate"] ?? throw new InvalidOperationException("Agora:AppCertificate not configured.");

        var expiresAt = DateTime.UtcNow.AddHours(4);
        // Real implementation: AgoraIO.Media.RtcTokenBuilder.buildTokenWithUid(...)
        var token = GenerateRtcToken(appId, appCertificate, channelName, userId, role, expiresAt);

        return Task.FromResult(new AgoraToken(token, channelName, role, expiresAt));
    }

    public async Task<long> GetCurrentMonthUsageMinutesAsync(CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(QuotaKey);
        return value.HasValue && long.TryParse(value, out var minutes) ? minutes : 0;
    }

    public Task RevokeChannelAsync(string channelName, CancellationToken ct = default)
    {
        if (MockMode)
        {
            _logger.LogDebug("Agora MockMode: skipping channel revocation for {Channel}", channelName);
            return Task.CompletedTask;
        }

        // TODO: call Agora Channel Management API to kick all users
        _logger.LogInformation("Revoking Agora channel {Channel}", channelName);
        return Task.CompletedTask;
    }

    private static string GenerateRtcToken(string appId, string appCertificate, string channelName,
        Guid userId, AgoraRole role, DateTime expiresAt)
    {
        // Placeholder — replace with actual Agora RTC Token Builder
        // Reference: https://docs.agora.io/en/video-calling/get-started/authentication-workflow
        var expireTimestamp = (uint)new DateTimeOffset(expiresAt).ToUnixTimeSeconds();
        return $"agora-rtc-{appId[..8]}-{channelName}-{userId:N[..8]}-{expireTimestamp}";
    }
}
