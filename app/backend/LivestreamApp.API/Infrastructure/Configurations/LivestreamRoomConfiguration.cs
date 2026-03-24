using LivestreamApp.Livestream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public sealed class LivestreamRoomConfiguration : IEntityTypeConfiguration<LivestreamRoom>
{
    public void Configure(EntityTypeBuilder<LivestreamRoom> builder)
    {
        builder.ToTable("livestream_rooms");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.HostId).IsRequired();
        builder.Property(r => r.Title).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Category).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.AgoraChannelName).HasMaxLength(64).IsRequired();
        builder.Property(r => r.ViewerCount).HasDefaultValue(0);
        builder.Property(r => r.PeakViewerCount).HasDefaultValue(0);
        builder.Property(r => r.TotalViewerCount).HasDefaultValue(0);
        builder.Property(r => r.CreatedAt).IsRequired();

        // BR-LS-01: Only 1 live room per host (partial unique index)
        builder.HasIndex(r => r.HostId)
            .HasFilter("\"Status\" = 'Live'")
            .IsUnique()
            .HasDatabaseName("idx_livestream_rooms_host_live_unique");

        builder.HasIndex(r => r.Status).HasDatabaseName("idx_livestream_rooms_status");

        builder.HasMany(r => r.ViewerSessions)
            .WithOne()
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.KickedViewers)
            .WithOne()
            .HasForeignKey(k => k.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
