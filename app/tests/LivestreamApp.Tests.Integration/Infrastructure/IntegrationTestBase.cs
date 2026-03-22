using LivestreamApp.API.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace LivestreamApp.Tests.Integration.Infrastructure;

/// <summary>
/// Base class for integration tests. Provides HttpClient and DB access.
/// Each test class gets a fresh scope; DB is migrated once per collection.
/// </summary>
[Collection("Integration")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly IntegrationTestFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(IntegrationTestFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    public async Task InitializeAsync()
    {
        // Migrate DB once — idempotent
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        // Clean all tables before each test for isolation
        await ResetDatabaseAsync(db);
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    private static async Task ResetDatabaseAsync(AppDbContext db)
    {
        // Delete in FK-safe order
        db.ExternalLogins.RemoveRange(db.ExternalLogins);
        db.RefreshTokens.RemoveRange(db.RefreshTokens);
        db.OtpCodes.RemoveRange(db.OtpCodes);
        db.UserPhotos.RemoveRange(db.UserPhotos);
        db.UserProfiles.RemoveRange(db.UserProfiles);
        db.HostProfiles.RemoveRange(db.HostProfiles);
        db.Users.RemoveRange(db.Users);
        await db.SaveChangesAsync();
    }

    /// <summary>Helper to POST JSON and return the response.</summary>
    protected Task<HttpResponseMessage> PostAsync<T>(string url, T body) =>
        Client.PostAsJsonAsync(url, body);

    /// <summary>Helper to GET and deserialize response body.</summary>
    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}
