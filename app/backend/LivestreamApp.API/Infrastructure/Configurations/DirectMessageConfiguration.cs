using LivestreamApp.DirectChat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public sealed class DirectMessageConfiguration : IEntityTypeConfiguration<DirectMessage>
{
    public void Configure(EntityTypeBuilder<DirectMessage> builder)
    {
        builder.ToTable("direct_messages");
        builder.HasKey(m => new { m.Id, m.SentAt }); // Composite PK for partitioned table

        builder.Property(m => m.ConversationId).IsRequired();
        builder.Property(m => m.SenderId).IsRequired();
        builder.Property(m => m.Content).HasMaxLength(1000).IsRequired();
        builder.Property(m => m.MessageType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(m => m.EmojiCode).HasMaxLength(50);
        builder.Property(m => m.IsRead).HasDefaultValue(false);
        builder.Property(m => m.IsDeletedBySender).HasDefaultValue(false);
        builder.Property(m => m.SentAt).IsRequired();

        // Index for efficient conversation message queries (includes partition key)
        builder.HasIndex(m => new { m.ConversationId, m.SentAt })
            .HasDatabaseName("idx_direct_messages_conversation_sent");
    }
}
