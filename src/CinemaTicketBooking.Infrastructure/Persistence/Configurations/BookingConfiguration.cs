using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.CustomerName).HasMaxLength(ColumnMaxLengths.Name).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(ColumnMaxLengths.PhoneNumber);
        builder.Property(x => x.Email).HasMaxLength(ColumnMaxLengths.Email);
        builder.Property(x => x.OriginAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.FinalAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.QrCode).HasMaxLength(ColumnMaxLengths.QrCode);
        builder.Property(x => x.Status).IsRequired();

        builder.HasOne(x => x.ShowTime)
            .WithMany()
            .HasForeignKey(x => x.ShowTimeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Tickets)
            .WithOne()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Concessions)
            .WithOne()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
