using System.Security.Claims;
using CinemaTicketBooking.Application.Common.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CinemaTicketBooking.Infrastructure.Auth;

/// <summary>
/// Seeds default Identity roles and admin permission role claims.
/// </summary>
public static class IdentityDataSeeder
{
    /// <summary>
    /// Ensures roles exist, admin role has static permission claims, and default accounts are seeded.
    /// </summary>
    public static async Task SeedAsync(
        RoleManager<Role> roleManager, 
        UserManager<Account> userManager, 
        ILogger logger,
        IConfiguration configuration,
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
                    Permissions.ConcessionsView, Permissions.ConcessionsManage,
                    Permissions.ShowTimesView,
                    Permissions.MoviesView
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

        // 3. Seed Default Users
        await SeedDefaultUserAsync(userManager, configuration, "SysAdmin", RoleNames.SystemAdmin, "sysadmin", "sysadmin@cinema.com", "SysAdmin@123!", logger);
        await SeedDefaultUserAsync(userManager, configuration, "Admin", RoleNames.Admin, "admin", "admin@cinema.com", "Admin@123!", logger);
        await SeedDefaultUserAsync(userManager, configuration, "Manager", RoleNames.Manager, "manager", "manager@cinema.com", "Manager@123!", logger);
        await SeedDefaultUserAsync(userManager, configuration, "Coordinator", RoleNames.MovieCoordinator, "coordinator", "coordinator@cinema.com", "Coo@123!", logger);
        await SeedDefaultUserAsync(userManager, configuration, "TicketStaff", RoleNames.TicketStaff, "staff", "staff@cinema.com", "Staff@123!", logger);
        await SeedDefaultUserAsync(userManager, configuration, "Customer", RoleNames.Customer, "customer", "customer@cinema.com", "Customer@123!", logger);
    }

    // =============================================
    // Helper Methods for Data Seeding
    // =============================================

    /// <summary>
    /// Seeds a default user if they do not already exist based on the configuration.
    /// </summary>
    private static async Task SeedDefaultUserAsync(
        UserManager<Account> userManager,
        IConfiguration configuration,
        string configKey,
        string roleName,
        string fallbackUsername,
        string fallbackEmail,
        string fallbackPassword,
        ILogger logger)
    {
        var section = configuration.GetSection($"DefaultAccount:{configKey}");
        var email = section["Email"] ?? fallbackEmail;
        var username = section["DefaultUserName"] ?? fallbackUsername;
        var password = section["DefaultPassword"] ?? fallbackPassword;

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new Account
            {
                Id = Guid.CreateVersion7(),
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                AvatarUrl = $"https://api.dicebear.com/7.x/adventurer/svg?seed={username}"
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, roleName);
                logger.LogInformation("Seeded {Role} user.", roleName);
            }
            else
            {
                logger.LogError("Failed to seed {Role} user: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
