using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class PricingPolicyConfiguration : IEntityTypeConfiguration<PricingPolicy>
{
    public void Configure(EntityTypeBuilder<PricingPolicy> builder)
    {
        builder.ToTable("pricing_policies");
        builder.ConfigureAggregateRoot();

        builder.Property(x => x.ScreenType).IsRequired();
        builder.Property(x => x.SeatType).IsRequired();
        builder.Property(x => x.BasePrice).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.ScreenCoefficient).HasPrecision(8, 3).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasOne(x => x.Cinema)
            .WithMany()
            .HasForeignKey(x => x.CinemaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
