using System.Linq.Expressions;
using CinemaTicketBooking.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Infrastructure.Persistence.Configurations;

public static class ModelBuilderConfigurationExtensions
{
    public static void ApplySoftDeleteQueryFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletalbe).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var deletedAtProperty = Expression.Property(parameter, nameof(ISoftDeletalbe.DeletedAt));
            var deletedAtIsNull = Expression.Equal(
                deletedAtProperty,
                Expression.Constant(null, typeof(DateTimeOffset?)));
            var lambda = Expression.Lambda(deletedAtIsNull, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    public static void ConfigureIdentityTextColumns(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityUserClaim<Guid>>(builder =>
        {
            builder.ToTable("account_claims");
            builder.Property(x => x.ClaimType).HasMaxLength(ColumnMaxLengths.ClaimType);
            builder.Property(x => x.ClaimValue).HasMaxLength(ColumnMaxLengths.ClaimValue);
        });

        modelBuilder.Entity<IdentityUserRole<Guid>>(builder =>
        {
            builder.ToTable("account_roles");
        });

        modelBuilder.Entity<IdentityUserLogin<Guid>>(builder =>
        {
            builder.ToTable("account_logins");
            builder.Property(x => x.LoginProvider).HasMaxLength(ColumnMaxLengths.IdentityProvider);
            builder.Property(x => x.ProviderKey).HasMaxLength(ColumnMaxLengths.IdentityProviderKey);
            builder.Property(x => x.ProviderDisplayName).HasMaxLength(ColumnMaxLengths.Name);
        });

        modelBuilder.Entity<IdentityRoleClaim<Guid>>(builder =>
        {
            builder.ToTable("role_claims");
            builder.Property(x => x.ClaimType).HasMaxLength(ColumnMaxLengths.ClaimType);
            builder.Property(x => x.ClaimValue).HasMaxLength(ColumnMaxLengths.ClaimValue);
        });

        modelBuilder.Entity<IdentityUserToken<Guid>>(builder =>
        {
            builder.ToTable("account_tokens");
            builder.Property(x => x.LoginProvider).HasMaxLength(ColumnMaxLengths.IdentityProvider);
            builder.Property(x => x.Name).HasMaxLength(ColumnMaxLengths.TokenName);
            builder.Property(x => x.Value).HasMaxLength(ColumnMaxLengths.TokenValue);
        });
    }
}
