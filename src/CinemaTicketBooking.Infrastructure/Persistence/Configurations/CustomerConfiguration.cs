using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.Name).HasMaxLength(ColumnMaxLengths.Name).IsRequired();
        builder.Property(x => x.SessionId).HasMaxLength(ColumnMaxLengths.SessionId).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(ColumnMaxLengths.PhoneNumber);
        builder.Property(x => x.Email).HasMaxLength(ColumnMaxLengths.Email);
        builder.Property(x => x.IsRegistered).IsRequired();
    }
}
