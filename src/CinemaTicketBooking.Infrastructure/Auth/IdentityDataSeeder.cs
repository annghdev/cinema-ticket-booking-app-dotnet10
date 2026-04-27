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
        var rolesToSeed = new Dictionary<string, string>
        {
            { RoleNames.SystemAdmin, "Quản trị hệ thống" },
            { RoleNames.Admin, "Quản trị viên" },
            { RoleNames.Manager, "Quản lý" },
            { RoleNames.MovieCoordinator, "Điều phối viên phim" },
            { RoleNames.TicketStaff, "Nhân viên quầy vé" },
            { RoleNames.Customer, "Khách hàng" }
        };

        foreach (var kvp in rolesToSeed)
        {
            var roleName = kvp.Key;
            var displayName = kvp.Value;

            if (await roleManager.RoleExistsAsync(roleName))
            {
                var existingRole = await roleManager.FindByNameAsync(roleName);
                if (existingRole != null && (string.IsNullOrEmpty(existingRole.DisplayName) || existingRole.DisplayName == roleName))
                {
                    existingRole.DisplayName = displayName;
                    await roleManager.UpdateAsync(existingRole);
                }
                continue;
            }

            var role = new Role
            {
                Id = Guid.CreateVersion7(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                DisplayName = displayName
            };
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create role {Role}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                continue;
            }

            logger.LogInformation("Created Identity role {Role}.", roleName);
        }

        // 2. Grant Claims dynamically
        var availablePermissions = typeof(Permissions)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(fi => (string)fi.GetValue(null)!)
            .ToList();

        var roleMappings = new Dictionary<string, List<string>>
        {
            { RoleNames.Admin, availablePermissions },
            { RoleNames.Manager, new List<string> 
                { 
                    Permissions.MoviesView, Permissions.MoviesManage,
                    Permissions.ShowTimesView, Permissions.ShowTimesManage,
                    Permissions.PricingPoliciesView, Permissions.PricingPoliciesManage,
                    Permissions.SeatSelectionPoliciesView, Permissions.SeatSelectionPoliciesManage,
                    Permissions.ConcessionsView, Permissions.ConcessionsManage,
                    Permissions.BookingsViewAll, Permissions.ReportsView, Permissions.AnalysisView
                } 
            },
            { RoleNames.MovieCoordinator, new List<string> 
                { 
                    Permissions.MoviesView, Permissions.MoviesManage,
                    Permissions.ShowTimesView, Permissions.ShowTimesManage 
                } 
            },
            { RoleNames.TicketStaff, new List<string> 
                { 
                    Permissions.BookingsViewAll, Permissions.BookingsManage,
                    Permissions.ConcessionsView, Permissions.ConcessionsManage 
                } 
            }
        };

        foreach (var mapping in roleMappings)
        {
            var r = await roleManager.FindByNameAsync(mapping.Key);
            if (r == null) continue;

            var existingClaims = await roleManager.GetClaimsAsync(r);
            var existingPerms = existingClaims
                .Where(c => c.Type == AuthClaimTypes.Permission)
                .Select(c => c.Value)
                .ToList();

            foreach (var perm in mapping.Value)
            {
                if (!existingPerms.Contains(perm))
                {
                    await roleManager.AddClaimAsync(r, new Claim(AuthClaimTypes.Permission, perm));
                }
            }
        }

        // 2.5 Seed System Admin User
        var sysEmail = "sysadmin@cinema.com";
        var sysUser = await userManager.FindByEmailAsync(sysEmail);
        if (sysUser is null)
        {
            sysUser = new Account
            {
                Id = Guid.CreateVersion7(),
                UserName = "sysadmin",
                Email = sysEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(sysUser, "SysAdmin@123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(sysUser, RoleNames.SystemAdmin);
                logger.LogInformation("Seeded System Admin user.");
            }
        }
        var adminEmail = "admin@cinema.com";
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
        var managerEmail = "manager@cinema.com";
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

        // 5. Seed Movie Coordinator User
        var coordinatorEmail = "coordinator@cinema.com";
        var coordinatorUser = await userManager.FindByEmailAsync(coordinatorEmail);
        if (coordinatorUser is null)
        {
            coordinatorUser = new Account
            {
                Id = Guid.CreateVersion7(),
                UserName = "coordinator",
                Email = coordinatorEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(coordinatorUser, "Coo@123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(coordinatorUser, RoleNames.MovieCoordinator);
                logger.LogInformation("Seeded Movie Coordinator user.");
            }
        }

        // 6. Seed Ticket Staff User
        var staffEmail = "staff@cinema.com";
        var staffUser = await userManager.FindByEmailAsync(staffEmail);
        if (staffUser is null)
        {
            staffUser = new Account
            {
                Id = Guid.CreateVersion7(),
                UserName = "staff",
                Email = staffEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(staffUser, "Staff@123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(staffUser, RoleNames.TicketStaff);
                logger.LogInformation("Seeded Ticket Staff user.");
            }
        }
    }
}
