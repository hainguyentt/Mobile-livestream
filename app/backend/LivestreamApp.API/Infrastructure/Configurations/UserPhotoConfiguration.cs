using LivestreamApp.Profiles.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public class UserPhotoConfiguration : IEntityTypeConfiguration<UserPhoto>
{
    public void Configure(EntityTypeBuilder<UserPhoto> builder)
    {
        builder.ToTable("user_photos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.S3Key).HasMaxLength(500).IsRequired();
        builder.Property(p => p.S3Url).HasMaxLength(1000).IsRequired();
        builder.Property(p => p.MimeType).HasMaxLength(50).IsRequired();

        builder.Property(p => p.UploadedAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // Unique: one display index per user
        builder.HasIndex(p => new { p.UserId, p.DisplayIndex }).IsUnique();
    }
}
