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

        builder.Property(x => x.Name).HasMaxLength(ColumnMaxLengths.Name).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(ColumnMaxLengths.Description);
        builder.Property(x => x.ThumbnailUrl).HasMaxLength(ColumnMaxLengths.Url);
        builder.Property(x => x.Studio).HasMaxLength(ColumnMaxLengths.Name);
        builder.Property(x => x.Director).HasMaxLength(ColumnMaxLengths.ActorName);
        builder.Property(x => x.OfficialTrailerUrl).HasMaxLength(ColumnMaxLengths.Url);
        builder.Property(x => x.Duration).IsRequired();
        builder.Property(x => x.Genre).IsRequired();
        builder.Property(x => x.Status).IsRequired();
    }
}
