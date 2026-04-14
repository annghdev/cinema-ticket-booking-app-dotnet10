using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("movies");
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.Name).HasMaxLength(MaxLengthConsts.Name).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(MaxLengthConsts.Description);
        builder.Property(x => x.ThumbnailUrl).HasMaxLength(MaxLengthConsts.Url);
        builder.Property(x => x.Studio).HasMaxLength(MaxLengthConsts.Name);
        builder.Property(x => x.Director).HasMaxLength(MaxLengthConsts.ActorName);
        builder.Property(x => x.OfficialTrailerUrl).HasMaxLength(MaxLengthConsts.Url);
        builder.Property(x => x.Duration).IsRequired();
        builder.Property(x => x.Genre).IsRequired();
        builder.Property(x => x.Status).IsRequired();
    }
}
