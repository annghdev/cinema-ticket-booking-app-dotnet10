using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public static class BaseEntityConfigurationExtensions
{
    public static void ConfigureDefaultEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IDefaultEntity
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion()
            .ValueGeneratedOnAddOrUpdate();
        builder.Ignore(x => x.Events);
    }

    public static void ConfigureTrackable<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, ITrackable
    {
        builder.Property(x => x.CreatedBy).HasMaxLength(MaxLengthConsts.ActorName);
        builder.Property(x => x.UpdatedBy).HasMaxLength(MaxLengthConsts.ActorName);
        builder.Property(x => x.UpdatedAt);
    }

    public static void ConfigureSoftDeletable<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, ISoftDeletalbe
    {
        builder.Property(x => x.DeletedAt);
        builder.Ignore(x => x.IsDeleted);
    }

    public static void ConfigureAuditableEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity
    {
        builder.ConfigureDefaultEntity();
        builder.ConfigureTrackable();
        builder.ConfigureSoftDeletable();
    }
}
