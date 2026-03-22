using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LivestreamApp.API.Infrastructure;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Used by dotnet-ef tools when the application DI container is not available.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Use a placeholder connection string for design-time — actual connection comes from appsettings at runtime
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=livestream_dev;Username=postgres;Password=postgres",
            npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

        return new AppDbContext(optionsBuilder.Options);
    }
}
