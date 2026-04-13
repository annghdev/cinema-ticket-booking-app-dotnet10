using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.Code).HasMaxLength(ColumnMaxLengths.TicketCode).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(ColumnMaxLengths.TicketDescription);
        builder.Property(x => x.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.LockingBy).HasMaxLength(ColumnMaxLengths.SessionId);
        builder.Property(x => x.Status).IsRequired();

        builder.HasOne(x => x.ShowTime)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.ShowTimeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
