using LivestreamApp.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash).HasMaxLength(255).IsRequired();
        builder.Property(t => t.ReplacedByTokenHash).HasMaxLength(255);
        builder.Property(t => t.IpAddress).HasMaxLength(45).IsRequired();

        builder.Property(t => t.ExpiresAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.Property(t => t.CreatedAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => t.UserId);
    }
}
