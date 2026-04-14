using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");
        builder.ConfigureAggregateRoot();

        builder.Property(x => x.Code).HasMaxLength(MaxLengthConsts.TicketCode).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(MaxLengthConsts.TicketDescription);
        builder.Property(x => x.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.LockingBy).HasMaxLength(MaxLengthConsts.SessionId);
        builder.Property(x => x.LockExpiresAt);
        builder.Property(x => x.PaymentExpiresAt);
        builder.Property(x => x.Status).IsRequired();
        builder.HasIndex(x => new { x.Status, x.LockExpiresAt });
        builder.HasIndex(x => new { x.Status, x.PaymentExpiresAt });

        builder.HasOne(x => x.ShowTime)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.ShowTimeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
