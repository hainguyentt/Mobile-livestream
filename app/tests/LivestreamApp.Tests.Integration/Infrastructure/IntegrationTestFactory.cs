using LivestreamApp.API.Infrastructure;
using LivestreamApp.Auth.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace LivestreamApp.Tests.Integration.Infrastructure;

/// <summary>
/// Shared WebApplicationFactory that spins up real PostgreSQL and Redis containers.
/// Implements IAsyncLifetime so containers start/stop once per test collection.
/// </summary>
public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("livestream_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    /// <summary>Shared TestEmailService instance — accessible from tests via Factory.EmailService.</summary>
    public TestEmailService EmailService { get; } = new();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove all DbContext-related registrations
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                         || d.ServiceType == typeof(DbContextOptions)
                         || d.ServiceType == typeof(AppDbContext))
                .ToList();
            foreach (var d in descriptors)
                services.Remove(d);

            // Re-register with test container connection string
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString(),
                    npgsql => npgsql.EnableRetryOnFailure(3)));

            // Replace Redis connection string with test container
            var redisDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(StackExchange.Redis.IConnectionMultiplexer));
            if (redisDescriptor is not null)
                services.Remove(redisDescriptor);

            services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(_ =>
                StackExchange.Redis.ConnectionMultiplexer.Connect(_redis.GetConnectionString()));

            // Replace email service with test capture implementation
            // Use Replace to ensure only one registration exists regardless of how many were added
            services.Replace(ServiceDescriptor.Singleton<IEmailService>(EmailService));
        });
    }

    public async new Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }
}
