namespace LivestreamApp.Livestream.Services;

/// <summary>Abstraction over coin wallet operations. Implementation lives in LivestreamApp.API (DB-backed).</summary>
public interface ICoinWalletService
{
    Task<int> GetBalanceAsync(Guid userId, CancellationToken ct = default);
    Task DeductAsync(Guid userId, int coins, CancellationToken ct = default);
    Task AddAsync(Guid userId, int coins, CancellationToken ct = default);
}
