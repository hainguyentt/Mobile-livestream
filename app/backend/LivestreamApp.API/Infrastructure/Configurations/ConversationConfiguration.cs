using LivestreamApp.DirectChat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ViewerId).IsRequired();
        builder.Property(c => c.HostId).IsRequired();
        builder.Property(c => c.LastMessagePreview).HasMaxLength(100);
        builder.Property(c => c.ViewerUnreadCount).HasDefaultValue(0);
        builder.Property(c => c.HostUnreadCount).HasDefaultValue(0);
        builder.Property(c => c.IsHiddenByViewer).HasDefaultValue(false);
        builder.Property(c => c.IsHiddenByHost).HasDefaultValue(false);
        builder.Property(c => c.CreatedAt).IsRequired();

        // Unique: 1 conversation per Viewer-Host pair
        builder.HasIndex(c => new { c.ViewerId, c.HostId })
            .IsUnique()
            .HasDatabaseName("idx_conversations_viewer_host_unique");

        builder.HasIndex(c => c.ViewerId).HasDatabaseName("idx_conversations_viewer");
        builder.HasIndex(c => c.HostId).HasDatabaseName("idx_conversations_host");
    }
}
