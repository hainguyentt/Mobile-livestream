using LivestreamApp.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.ToTable("external_logins");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Provider).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ProviderUserId).HasMaxLength(255).IsRequired();
        builder.Property(e => e.ProviderEmail).HasMaxLength(255);

        builder.Property(e => e.CreatedAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // Unique: one LINE account can only link to one user
        builder.HasIndex(e => new { e.Provider, e.ProviderUserId }).IsUnique();
    }
}
