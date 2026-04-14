using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class ConcessionConfiguration : IEntityTypeConfiguration<Concession>
{
    public void Configure(EntityTypeBuilder<Concession> builder)
    {
        builder.ToTable("concessions");
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.Name).HasMaxLength(MaxLengthConsts.Name).IsRequired();
        builder.Property(x => x.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.ImageUrl).HasMaxLength(MaxLengthConsts.Url);
        builder.Property(x => x.IsAvailable).IsRequired();
    }
}
