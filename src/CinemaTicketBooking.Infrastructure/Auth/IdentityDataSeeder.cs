using System.Security.Claims;
using CinemaTicketBooking.Application.Common.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CinemaTicketBooking.Infrastructure.Auth;

/// <summary>
/// Seeds default Identity roles and admin permission role claims.
/// </summary>
public static class IdentityDataSeeder
{
    private static readonly Claim[] AdminPermissionClaims =
    [
        new(AuthClaimTypes.Permission, Permissions.BookingsViewAll),
        new(AuthClaimTypes.Permission, Permissions.AccountsLock),
        new(AuthClaimTypes.Permission, Permissions.AccountsUnlock)
    ];

    /// <summary>
    /// Ensures roles exist and admin role has static permission claims.
    /// </summary>
    public static async Task SeedAsync(RoleManager<Role> roleManager, ILogger logger, CancellationToken cancellationToken = default)
    {
        foreach (var roleName in new[] { RoleNames.Customer, RoleNames.Admin })
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var role = new Role
            {
                Id = Guid.CreateVersion7(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                DisplayName = roleName
            };
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create role {Role}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                continue;
            }

            logger.LogInformation("Created Identity role {Role}.", roleName);
        }

        var adminRole = await roleManager.Roles.FirstOrDefaultAsync(
            x => x.NormalizedName == RoleNames.Admin.ToUpperInvariant(),
            cancellationToken);
        if (adminRole is null)
            return;

        var existingClaims = await roleManager.GetClaimsAsync(adminRole);
        foreach (var claim in AdminPermissionClaims)
        {
            if (existingClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                continue;

            var add = await roleManager.AddClaimAsync(adminRole, claim);
            if (!add.Succeeded)
            {
                logger.LogError(
                    "Failed to add claim {Type}={Value} to Admin: {Errors}",
                    claim.Type,
                    claim.Value,
                    string.Join(", ", add.Errors.Select(e => e.Description)));
            }
        }
    }
}
