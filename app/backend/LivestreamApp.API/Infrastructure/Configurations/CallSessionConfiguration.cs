using LivestreamApp.Livestream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public sealed class CallSessionConfiguration : IEntityTypeConfiguration<CallSession>
{
    public void Configure(EntityTypeBuilder<CallSession> builder)
    {
        builder.ToTable("call_sessions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.CallRequestId).IsRequired();
        builder.Property(s => s.ViewerId).IsRequired();
        builder.Property(s => s.HostId).IsRequired();
        builder.Property(s => s.AgoraChannelName).HasMaxLength(64).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(s => s.CoinRatePerTick).IsRequired();
        builder.Property(s => s.TotalCoinsCharged).HasDefaultValue(0);
        builder.Property(s => s.TotalTicks).HasDefaultValue(0);
        builder.Property(s => s.StartedAt).IsRequired();
        builder.Property(s => s.EndedBy).HasMaxLength(20);

        // 1 call session per call request
        builder.HasIndex(s => s.CallRequestId).IsUnique()
            .HasDatabaseName("idx_call_sessions_request_unique");

        builder.HasMany(s => s.BillingTicks)
            .WithOne()
            .HasForeignKey(t => t.CallSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
