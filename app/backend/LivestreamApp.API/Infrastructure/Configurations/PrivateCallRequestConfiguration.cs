using LivestreamApp.Livestream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public sealed class PrivateCallRequestConfiguration : IEntityTypeConfiguration<PrivateCallRequest>
{
    public void Configure(EntityTypeBuilder<PrivateCallRequest> builder)
    {
        builder.ToTable("private_call_requests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ViewerId).IsRequired();
        builder.Property(r => r.HostId).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.CoinRatePerTick).IsRequired();
        builder.Property(r => r.RequestedAt).IsRequired();
        builder.Property(r => r.ExpiresAt).IsRequired();

        builder.HasIndex(r => r.HostId)
            .HasFilter("\"Status\" = 'Pending'")
            .IsUnique()
            .HasDatabaseName("idx_call_requests_host_pending_unique");

        builder.HasIndex(r => r.ViewerId).HasDatabaseName("idx_call_requests_viewer");
    }
}
