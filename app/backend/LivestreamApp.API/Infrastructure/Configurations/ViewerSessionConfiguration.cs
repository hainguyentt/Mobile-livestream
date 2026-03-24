using LivestreamApp.Livestream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public sealed class ViewerSessionConfiguration : IEntityTypeConfiguration<ViewerSession>
{
    public void Configure(EntityTypeBuilder<ViewerSession> builder)
    {
        builder.ToTable("viewer_sessions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.RoomId).IsRequired();
        builder.Property(s => s.ViewerId).IsRequired();
        builder.Property(s => s.JoinedAt).IsRequired();
        builder.Property(s => s.WatchDurationSeconds).HasDefaultValue(0);
        builder.Property(s => s.IsKicked).HasDefaultValue(false);

        // Unique active session per viewer per room
        builder.HasIndex(s => new { s.RoomId, s.ViewerId })
            .HasFilter("\"LeftAt\" IS NULL")
            .IsUnique()
            .HasDatabaseName("idx_viewer_sessions_active_unique");

        builder.HasIndex(s => s.RoomId).HasDatabaseName("idx_viewer_sessions_room");
        builder.HasIndex(s => s.ViewerId).HasDatabaseName("idx_viewer_sessions_viewer");
    }
}
