using LivestreamApp.Livestream.Services;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure.Services;

/// <summary>
/// Coin wallet service backed by PostgreSQL.
/// Coin balance is stored in user_profiles.coin_balance.
/// </summary>
public sealed class CoinWalletService : ICoinWalletService
{
    private readonly AppDbContext _db;
    private readonly ILogger<CoinWalletService> _logger;

    public CoinWalletService(AppDbContext db, ILogger<CoinWalletService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<int> GetBalanceAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.Id == userId, ct);
        return profile?.CoinBalance ?? 0;
    }

    public async Task DeductAsync(Guid userId, int coins, CancellationToken ct = default)
    {
        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.Id == userId, ct);
        if (profile == null) throw new InvalidOperationException($"User profile not found for {userId}");
        if (profile.CoinBalance < coins) throw new InvalidOperationException("Insufficient coin balance.");

        profile.DeductCoins(coins);
        await _db.SaveChangesAsync(ct);
        _logger.LogDebug("Deducted {Coins} coins from user {UserId}, new balance: {Balance}", coins, userId, profile.CoinBalance);
    }

    public async Task AddAsync(Guid userId, int coins, CancellationToken ct = default)
    {
        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.Id == userId, ct);
        if (profile == null) throw new InvalidOperationException($"User profile not found for {userId}");

        profile.AddCoins(coins);
        await _db.SaveChangesAsync(ct);
    }
}
