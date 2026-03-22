using LivestreamApp.Shared.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace LivestreamApp.API.Infrastructure.Services;

/// <summary>Redis implementation of ICacheService using StackExchange.Redis.</summary>
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;

    public RedisCacheService(IConnectionMultiplexer redis) =>
        _db = redis.GetDatabase();

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(value!);
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        if (expiry is null)
            throw new ArgumentNullException(nameof(expiry), "Cache TTL is required. Never store without expiry.");

        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiry);
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, CancellationToken ct = default) =>
        await _db.KeyDeleteAsync(key);

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default) =>
        await _db.KeyExistsAsync(key);
}
