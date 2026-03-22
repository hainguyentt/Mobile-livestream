using LivestreamApp.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.ToTable("otp_codes");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Target).HasMaxLength(255).IsRequired();
        builder.Property(o => o.CodeHash).HasMaxLength(255).IsRequired();
        builder.Property(o => o.Purpose).IsRequired();

        builder.Property(o => o.ExpiresAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.Property(o => o.CreatedAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        builder.HasIndex(o => new { o.Target, o.Purpose });
    }
}
