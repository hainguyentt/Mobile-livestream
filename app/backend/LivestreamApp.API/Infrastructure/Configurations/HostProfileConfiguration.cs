using LivestreamApp.Profiles.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public class HostProfileConfiguration : IEntityTypeConfiguration<HostProfile>
{
    public void Configure(EntityTypeBuilder<HostProfile> builder)
    {
        builder.ToTable("host_profiles");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.VerificationNote).HasMaxLength(500);
        builder.Property(h => h.VerificationStatus).IsRequired();

        builder.Property(h => h.VerifiedAt)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);
        builder.Property(h => h.VerificationRequestedAt)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);
        builder.Property(h => h.CreatedAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        builder.HasIndex(h => h.VerificationStatus);
    }
}
