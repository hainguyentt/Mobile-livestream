using LivestreamApp.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LivestreamApp.API.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        // Explicit ValueConverter ensures EF applies conversion on both read and write
        var emailConverter = new ValueConverter<LivestreamApp.Shared.Domain.ValueObjects.Email, string>(
            email => email.Value,
            value => LivestreamApp.Shared.Domain.ValueObjects.Email.Create(value));

        builder.Property(u => u.Email)
            .HasConversion(emailConverter)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.PasswordHash).HasMaxLength(255);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.Role).IsRequired();
        builder.Property(u => u.Status).IsRequired();

        builder.Property(u => u.RequiresCaptcha).IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.Property(u => u.UpdatedAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.Property(u => u.LockoutUntil)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);
        builder.Property(u => u.LastLoginAt)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ExternalLogins)
            .WithOne()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
