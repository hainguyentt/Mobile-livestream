using LivestreamApp.Livestream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LivestreamApp.API.Infrastructure.Configurations;

public sealed class BillingTickConfiguration : IEntityTypeConfiguration<BillingTick>
{
    public void Configure(EntityTypeBuilder<BillingTick> builder)
    {
        builder.ToTable("billing_ticks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.CallSessionId).IsRequired();
        builder.Property(t => t.TickNumber).IsRequired();
        builder.Property(t => t.CoinsCharged).IsRequired();
        builder.Property(t => t.ViewerBalanceBefore).IsRequired();
        builder.Property(t => t.ViewerBalanceAfter).IsRequired();
        builder.Property(t => t.ProcessedAt).IsRequired();
        builder.Property(t => t.IsSuccess).IsRequired();

        // Idempotency: unique constraint for ON CONFLICT DO NOTHING
        builder.HasIndex(t => new { t.CallSessionId, t.TickNumber })
            .IsUnique()
            .HasDatabaseName("idx_billing_ticks_session_tick_unique");
    }
}
