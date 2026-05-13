using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class SlideConfiguration : IEntityTypeConfiguration<Slide>
{
    public void Configure(EntityTypeBuilder<Slide> builder)
    {
        builder.ToTable("slides");
        builder.ConfigureAggregateRoot();

        builder.Property(x => x.Title).IsRequired().HasMaxLength(MaxLengthConsts.Name);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(MaxLengthConsts.Description);
        builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(MaxLengthConsts.Url);
        builder.Property(x => x.TargetUrl).IsRequired().HasMaxLength(MaxLengthConsts.Url);
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
    }
}
