using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public class SeatSelectionPolicyConfiguration : IEntityTypeConfiguration<SeatSelectionPolicy>
{
    public void Configure(EntityTypeBuilder<SeatSelectionPolicy> builder)
    {
        builder.ToTable("seat_selection_policies");
        builder.ConfigureAggregateRoot();

        builder.Property(x => x.Name)
            .HasMaxLength(MaxLengthConsts.Name)
            .IsRequired();
        builder.Property(x => x.IsGlobalDefault).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.Property(x => x.MaxTicketsPerCheckout).IsRequired();
        builder.Property(x => x.MaxRowsPerCheckout).IsRequired();

        builder.Property(x => x.OrphanSeatLevel).IsRequired();
        builder.Property(x => x.CheckerboardLevel).IsRequired();
        builder.Property(x => x.SplitAcrossAisleLevel).IsRequired();
        builder.Property(x => x.IsolatedRowEndSingleLevel).IsRequired();
        builder.Property(x => x.MisalignedRowsLevel).IsRequired();

        builder.HasIndex(x => new { x.IsGlobalDefault, x.IsActive });
    }
}
