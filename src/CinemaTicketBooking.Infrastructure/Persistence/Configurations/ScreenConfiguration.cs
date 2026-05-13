using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class ScreenConfiguration : IEntityTypeConfiguration<Screen>
{
    public void Configure(EntityTypeBuilder<Screen> builder)
    {
        builder.ToTable("screens");
        builder.ConfigureAggregateRoot();

        builder.Property(x => x.Code).HasMaxLength(MaxLengthConsts.ScreenCode).IsRequired();
        builder.Property(x => x.RowOfSeats).IsRequired();
        builder.Property(x => x.ColumnOfSeats).IsRequired();
        builder.Property(x => x.TotalSeats).IsRequired();
        builder.Property(x => x.SeatMap).HasColumnType("text");
        builder.Property(x => x.SupportedFormats).HasColumnType("jsonb");
        builder.Ignore(x => x.Type);
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasOne(x => x.Cinema)
            .WithMany()
            .HasForeignKey(x => x.CinemaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Seats)
            .WithOne()
            .HasForeignKey("ScreenId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
