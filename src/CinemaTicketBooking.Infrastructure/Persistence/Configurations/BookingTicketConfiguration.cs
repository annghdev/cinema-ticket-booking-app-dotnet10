using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class BookingTicketConfiguration : IEntityTypeConfiguration<BookingTicket>
{
    public void Configure(EntityTypeBuilder<BookingTicket> builder)
    {
        builder.ToTable("booking_tickets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.BookingId).IsRequired();
        builder.Property(x => x.TicketId).IsRequired();

        builder.HasOne(x => x.Ticket)
            .WithMany()
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
