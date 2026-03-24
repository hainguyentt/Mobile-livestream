using LivestreamApp.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.HostedServices;

/// <summary>
/// Startup service: ensures PostgreSQL partitions exist for direct_messages.
/// Creates partitions for current month + 2 months ahead.
/// </summary>
public sealed class PartitionMaintenanceService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PartitionMaintenanceService> _logger;

    public PartitionMaintenanceService(IServiceScopeFactory scopeFactory, ILogger<PartitionMaintenanceService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;
        for (var i = 0; i <= 2; i++)
        {
            var month = now.AddMonths(i);
            await EnsurePartitionExistsAsync(db, "direct_messages", month, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task EnsurePartitionExistsAsync(AppDbContext db, string table, DateTime month, CancellationToken ct)
    {
        // Use a direct check via SQL scalar — table name is internal constant, not user input
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync(ct);
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = @table AND table_schema = 'public'";
            var param = cmd.CreateParameter();
            param.ParameterName = "@table";
            param.Value = table;
            cmd.Parameters.Add(param);
            var count = (long)(await cmd.ExecuteScalarAsync(ct) ?? 0L);
            if (count == 0)
            {
                _logger.LogWarning("Table {Table} does not exist yet — skipping partition creation (migration pending)", table);
                return;
            }
        }
        finally
        {
            await conn.CloseAsync();
        }

        var partitionName = $"{table}_{month:yyyy_MM}";
        var startDate = new DateTime(month.Year, month.Month, 1);
        var endDate = startDate.AddMonths(1);

        // Partition names and dates are derived from internal constants — not user input
        // Suppressing EF1002: table/partition names cannot be parameterized in DDL statements
#pragma warning disable EF1002
        var sql = $"""
            CREATE TABLE IF NOT EXISTS {partitionName}
            PARTITION OF {table}
            FOR VALUES FROM ('{startDate:yyyy-MM-dd}') TO ('{endDate:yyyy-MM-dd}')
            """;

        try
        {
            await db.Database.ExecuteSqlRawAsync(sql, ct);
#pragma warning restore EF1002
            _logger.LogInformation("Partition {PartitionName} ensured for table {Table}", partitionName, table);
        }
        catch (Exception ex)
        {
            // Log but don't crash — partition may already exist or table may not be partitioned yet
            _logger.LogWarning(ex, "Could not ensure partition {PartitionName} — will retry on next startup", partitionName);
        }
    }
}
