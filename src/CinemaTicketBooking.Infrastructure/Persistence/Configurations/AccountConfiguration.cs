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
        builder.ConfigureAuditable();
        builder.ConfigureSoftDeletable();

        builder.Property(x => x.Email).HasMaxLength(MaxLengthConsts.Email);
        builder.Property(x => x.PhoneNumber).HasMaxLength(MaxLengthConsts.PhoneNumber);
        builder.Property(x => x.UserName).HasMaxLength(MaxLengthConsts.Email);
        builder.Property(x => x.NormalizedUserName).HasMaxLength(MaxLengthConsts.Email);
        builder.Property(x => x.NormalizedEmail).HasMaxLength(MaxLengthConsts.Email);
        builder.Property(x => x.PasswordHash).HasMaxLength(MaxLengthConsts.PasswordHash);
        builder.Property(x => x.SecurityStamp).HasMaxLength(MaxLengthConsts.SecurityStamp);
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(MaxLengthConsts.ConcurrencyStamp);
    }
}
