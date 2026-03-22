using LivestreamApp.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    public void Configure(EntityTypeBuilder<LoginAttempt> builder)
    {
        builder.ToTable("login_attempts");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Email).HasMaxLength(255).IsRequired();
        builder.Property(l => l.IpAddress).HasMaxLength(45).IsRequired();
        builder.Property(l => l.FailureReason).HasMaxLength(100);

        builder.Property(l => l.AttemptedAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        builder.HasIndex(l => new { l.Email, l.AttemptedAt });
        builder.HasIndex(l => new { l.IpAddress, l.AttemptedAt });
    }
}
