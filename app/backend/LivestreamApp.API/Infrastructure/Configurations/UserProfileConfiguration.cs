using LivestreamApp.Profiles.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Bio).HasMaxLength(500);
        builder.Property(p => p.PreferredLanguage).HasMaxLength(5).IsRequired();

        // Store string[] as PostgreSQL native text array
        builder.Property(p => p.Interests)
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'::text[]");

        builder.Property(p => p.CreatedAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.Property(p => p.UpdatedAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // Case-insensitive unique index using PostgreSQL lower() function
        builder.HasIndex(p => p.DisplayName)
            .IsUnique()
            .HasDatabaseName("ix_user_profiles_display_name_lower")
            .HasFilter(null);

        // EF Core doesn't support expression indexes natively — use raw SQL in migration:
        // CREATE UNIQUE INDEX ix_user_profiles_display_name_lower ON user_profiles (lower(display_name));

        builder.HasMany(p => p.Photos)
            .WithOne()
            .HasForeignKey(ph => ph.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
