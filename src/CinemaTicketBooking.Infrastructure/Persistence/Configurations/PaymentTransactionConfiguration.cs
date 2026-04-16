using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("payment_transactions");
        builder.ConfigureDefaultEntity();

        builder.Property(x => x.Method).IsRequired();
        builder.Property(x => x.GatewayTransactionId).HasMaxLength(MaxLengthConsts.GatewayTransactionId);
        builder.Property(x => x.RedirectBehavior).IsRequired();
        builder.Property(x => x.PaymentUrl).HasMaxLength(MaxLengthConsts.PaymentUrl);
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.GatewayResponseRaw).HasMaxLength(MaxLengthConsts.GatewayResponseRaw);
        builder.Property(x => x.PaidAt);
        builder.Property(x => x.ExpiresAt).IsRequired();

        builder.HasOne(x => x.Booking)
            .WithMany()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
