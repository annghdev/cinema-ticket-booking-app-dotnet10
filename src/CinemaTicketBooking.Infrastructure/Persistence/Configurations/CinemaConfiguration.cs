using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class CinemaConfiguration : IEntityTypeConfiguration<Cinema>
{
    public void Configure(EntityTypeBuilder<Cinema> builder)
    {
        builder.ToTable("cinemas");
        builder.ConfigureAggregateRoot();

        builder.Property(x => x.Name).HasMaxLength(MaxLengthConsts.Name).IsRequired();
        builder.Property(x => x.ThumbnailUrl).HasMaxLength(MaxLengthConsts.Url);
        builder.Property(x => x.Geo).HasMaxLength(MaxLengthConsts.Geo);
        builder.Property(x => x.Address).HasMaxLength(MaxLengthConsts.Address).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasMany<Screen>()
            .WithOne(x => x.Cinema)
            .HasForeignKey(x => x.CinemaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
