using LivestreamApp.Livestream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public sealed class KickedViewerConfiguration : IEntityTypeConfiguration<KickedViewer>
{
    public void Configure(EntityTypeBuilder<KickedViewer> builder)
    {
        builder.ToTable("kicked_viewers");
        builder.HasKey(k => k.Id);

        builder.Property(k => k.RoomId).IsRequired();
        builder.Property(k => k.ViewerId).IsRequired();
        builder.Property(k => k.KickedByUserId).IsRequired();
        builder.Property(k => k.KickedByRole).HasMaxLength(20).IsRequired();
        builder.Property(k => k.Reason).HasMaxLength(500);
        builder.Property(k => k.KickedAt).IsRequired();

        // Unique: 1 kick record per viewer per room
        builder.HasIndex(k => new { k.RoomId, k.ViewerId })
            .IsUnique()
            .HasDatabaseName("idx_kicked_viewers_room_viewer_unique");
    }
}
