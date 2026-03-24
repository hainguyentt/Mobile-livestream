using LivestreamApp.DirectChat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public sealed class BlockConfiguration : IEntityTypeConfiguration<Block>
{
    public void Configure(EntityTypeBuilder<Block> builder)
    {
        builder.ToTable("blocks");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BlockerId).IsRequired();
        builder.Property(b => b.BlockedId).IsRequired();
        builder.Property(b => b.BlockedAt).IsRequired();

        builder.HasIndex(b => new { b.BlockerId, b.BlockedId })
            .IsUnique()
            .HasDatabaseName("idx_blocks_blocker_blocked_unique");

        builder.HasIndex(b => b.BlockerId).HasDatabaseName("idx_blocks_blocker");
        builder.HasIndex(b => b.BlockedId).HasDatabaseName("idx_blocks_blocked");
    }
}
