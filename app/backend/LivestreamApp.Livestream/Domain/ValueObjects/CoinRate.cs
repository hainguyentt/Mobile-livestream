using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.Livestream.Domain.ValueObjects;

/// <summary>Coin billing rate for a private call session.</summary>
public sealed record CoinRate(int CoinsPerTick, int TickIntervalSeconds = 10)
{
    public int CoinsPerMinute => CoinsPerTick * (60 / TickIntervalSeconds);

    public static CoinRate Create(int coinsPerTick, int tickIntervalSeconds = 10)
    {
        if (coinsPerTick <= 0) throw new DomainException("CoinsPerTick must be positive.");
        if (tickIntervalSeconds <= 0) throw new DomainException("TickIntervalSeconds must be positive.");
        return new CoinRate(coinsPerTick, tickIntervalSeconds);
    }
}
