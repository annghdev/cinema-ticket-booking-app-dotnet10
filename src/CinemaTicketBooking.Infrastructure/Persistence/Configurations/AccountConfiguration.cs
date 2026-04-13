using CinemaTicketBooking.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.ConfigureDefaultEntity();
        builder.ConfigureTrackable();
        builder.ConfigureSoftDeletable();

        builder.Property(x => x.Email).HasMaxLength(ColumnMaxLengths.Email);
        builder.Property(x => x.PhoneNumber).HasMaxLength(ColumnMaxLengths.PhoneNumber);
        builder.Property(x => x.UserName).HasMaxLength(ColumnMaxLengths.Email);
        builder.Property(x => x.NormalizedUserName).HasMaxLength(ColumnMaxLengths.Email);
        builder.Property(x => x.NormalizedEmail).HasMaxLength(ColumnMaxLengths.Email);
        builder.Property(x => x.PasswordHash).HasMaxLength(ColumnMaxLengths.PasswordHash);
        builder.Property(x => x.SecurityStamp).HasMaxLength(ColumnMaxLengths.SecurityStamp);
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(ColumnMaxLengths.ConcurrencyStamp);
    }
}
