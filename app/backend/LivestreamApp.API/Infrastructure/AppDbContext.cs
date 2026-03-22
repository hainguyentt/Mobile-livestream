using LivestreamApp.Auth.Domain.Entities;
using LivestreamApp.Profiles.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure;

/// <summary>Write context — use for INSERT, UPDATE, DELETE operations only.</summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<HostProfile> HostProfiles => Set<HostProfile>();
    public DbSet<UserPhoto> UserPhotos => Set<UserPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration<T> from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
