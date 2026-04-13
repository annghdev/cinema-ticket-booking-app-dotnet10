using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class ShowTimeConfiguration : IEntityTypeConfiguration<ShowTime>
{
    public void Configure(EntityTypeBuilder<ShowTime> builder)
    {
        builder.ToTable("show_times");
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.StartAt).IsRequired();
        builder.Property(x => x.EndAt).IsRequired();
        builder.Property(x => x.Status).IsRequired();

        builder.HasOne(x => x.Movie)
            .WithMany()
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Screen)
            .WithMany()
            .HasForeignKey(x => x.ScreenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Tickets)
            .WithOne(x => x.ShowTime)
            .HasForeignKey(x => x.ShowTimeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
