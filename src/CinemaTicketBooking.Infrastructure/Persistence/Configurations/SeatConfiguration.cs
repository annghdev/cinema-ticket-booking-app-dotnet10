using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("seats");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Code).HasMaxLength(ColumnMaxLengths.SeatCode).IsRequired();
        builder.Property(x => x.Row).IsRequired();
        builder.Property(x => x.Column).IsRequired();
        builder.Property(x => x.IsAvailable).IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property<Guid>("ScreenId").IsRequired();
    }
}
