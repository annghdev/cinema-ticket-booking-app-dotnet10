using CinemaTicketBooking.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.ConfigureDefaultEntity();
        builder.ConfigureTrackable();
        builder.ConfigureSoftDeletable();

        builder.Property(x => x.DisplayName).HasMaxLength(ColumnMaxLengths.Name).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(ColumnMaxLengths.Name);
        builder.Property(x => x.NormalizedName).HasMaxLength(ColumnMaxLengths.Name);
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(ColumnMaxLengths.ConcurrencyStamp);
    }
}
