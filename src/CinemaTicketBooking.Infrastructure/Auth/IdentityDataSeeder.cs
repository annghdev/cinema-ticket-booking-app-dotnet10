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
    /// Ensures roles exist, admin role has static permission claims, and default accounts are seeded.
    /// </summary>
    public static async Task SeedAsync(
        RoleManager<Role> roleManager, 
        UserManager<Account> userManager, 
        ILogger logger, 
        CancellationToken cancellationToken = default)
    {
        // 1. Seed Roles
        foreach (var roleName in new[] { RoleNames.Customer, RoleNames.Admin, RoleNames.Manager })
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

        // 2. Grant Claims to Admin
        var adminRole = await roleManager.Roles.FirstOrDefaultAsync(
            x => x.NormalizedName == RoleNames.Admin.ToUpperInvariant(),
            cancellationToken);
        if (adminRole is not null)
        {
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

        // 3. Seed Admin User
        var adminEmail = "admin@aethercinema.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new Account
            {
                Id = Guid.CreateVersion7(),
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
                logger.LogInformation("Seeded Admin user.");
            }
            else
            {
                logger.LogError("Failed to seed Admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // 4. Seed Manager User
        var managerEmail = "manager@aethercinema.com";
        var managerUser = await userManager.FindByEmailAsync(managerEmail);
        if (managerUser is null)
        {
            managerUser = new Account
            {
                Id = Guid.CreateVersion7(),
                UserName = "manager",
                Email = managerEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(managerUser, "Manager@123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(managerUser, RoleNames.Manager);
                logger.LogInformation("Seeded Manager user.");
            }
            else
            {
                logger.LogError("Failed to seed Manager user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
