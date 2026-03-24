using LivestreamApp.Auth.Domain.Entities;
using LivestreamApp.DirectChat.Domain.Entities;
using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Profiles.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure;

/// <summary>Write context — use for INSERT, UPDATE, DELETE operations only.</summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Unit 1 — Auth
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();

    // Unit 1 — Profiles
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<HostProfile> HostProfiles => Set<HostProfile>();
    public DbSet<UserPhoto> UserPhotos => Set<UserPhoto>();

    // Unit 2 — Livestream
    public DbSet<LivestreamRoom> LivestreamRooms => Set<LivestreamRoom>();
    public DbSet<ViewerSession> ViewerSessions => Set<ViewerSession>();
    public DbSet<KickedViewer> KickedViewers => Set<KickedViewer>();
    public DbSet<PrivateCallRequest> PrivateCallRequests => Set<PrivateCallRequest>();
    public DbSet<CallSession> CallSessions => Set<CallSession>();
    public DbSet<BillingTick> BillingTicks => Set<BillingTick>();

    // Unit 2 — DirectChat
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<DirectMessage> DirectMessages => Set<DirectMessage>();
    public DbSet<Block> Blocks => Set<Block>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration<T> from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
