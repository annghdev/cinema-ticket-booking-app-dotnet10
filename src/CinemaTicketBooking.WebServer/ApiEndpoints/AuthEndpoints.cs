using System.Security.Claims;
using System.Security.Cryptography;
using CinemaTicketBooking.Application.Common.Auth;
using CinemaTicketBooking.Application.Features.Auth.Commands;
using CinemaTicketBooking.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Wolverine;

namespace CinemaTicketBooking.WebServer.ApiEndpoints;

/// <summary>
/// Maps authentication and account management minimal APIs.
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Registers <c>/api/auth</c> routes.
    /// </summary>
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", RegisterAsync)
            .AllowAnonymous();

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous();

        group.MapPost("/refresh", RefreshAsync)
            .AllowAnonymous();

        group.MapPost("/logout", LogoutAsync)
            .AllowAnonymous();

        group.MapPost("/forgot-password", ForgotPasswordAsync)
            .AllowAnonymous();

        group.MapPost("/reset-password", ResetPasswordAsync)
            .AllowAnonymous();

        group.MapDelete("/account", DeleteAccountAsync)
            .RequireAuthorization();

        group.MapGet("/external/google", GoogleChallenge)
            .AllowAnonymous();

        group.MapGet("/external/google/complete", GoogleCompleteAsync)
            .AllowAnonymous();

        group.MapGet("/external/facebook", FacebookChallenge)
            .AllowAnonymous();

        group.MapGet("/external/facebook/complete", FacebookCompleteAsync)
            .AllowAnonymous();

        var admin = group.MapGroup("/admin")
            .RequireAuthorization();

        admin.MapPost("/accounts/{accountId:guid}/lock", LockAccountAsync)
            .RequireAuthorization(Permissions.AccountsLock);

        admin.MapPost("/accounts/{accountId:guid}/unlock", UnlockAccountAsync)
            .RequireAuthorization(Permissions.AccountsUnlock);
    }

    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequest dto,
        UserManager<Account> userManager,
        IMessageBus bus,
        IIdentityAuthService auth,
        HttpContext http,
        CancellationToken ct)
    {
        var user = new Account
        {
            Id = Guid.CreateVersion7(),
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var create = await userManager.CreateAsync(user, dto.Password);
        if (!create.Succeeded)
            return Results.ValidationProblem(create.ErrorDictionary());

        var role = await userManager.AddToRoleAsync(user, RoleNames.Customer);
        if (!role.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return Results.ValidationProblem(role.ErrorDictionary());
        }

        try
        {
            await bus.InvokeAsync(
                new ProvisionCustomerForAccountCommand
                {
                    AccountId = user.Id,
                    Email = dto.Email,
                    Name = dto.Name,
                    PhoneNumber = dto.PhoneNumber,
                    CorrelationId = http.TraceIdentifier
                },
                ct);
        }
        catch
        {
            await userManager.DeleteAsync(user);
            throw;
        }

        var tokens = await auth.IssueTokensAsync(user, http.Connection.RemoteIpAddress?.ToString(), ct);
        if (tokens is null)
            return Results.Unauthorized();

        return Results.Ok(new AuthTokenApiResponse(
            tokens.AccessToken,
            tokens.AccessTokenExpiresAtUtc,
            tokens.AccountId,
            tokens.RefreshTokenPlaintext));
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest dto,
        SignInManager<Account> signInManager,
        UserManager<Account> userManager,
        IIdentityAuthService auth,
        HttpContext http,
        CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Results.Unauthorized();

        var check = await signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!check.Succeeded)
            return Results.Unauthorized();

        var tokens = await auth.IssueTokensAsync(user, http.Connection.RemoteIpAddress?.ToString(), ct);
        if (tokens is null)
            return Results.Unauthorized();

        return Results.Ok(new AuthTokenApiResponse(
            tokens.AccessToken,
            tokens.AccessTokenExpiresAtUtc,
            tokens.AccountId,
            tokens.RefreshTokenPlaintext));
    }

    private static async Task<IResult> RefreshAsync(
        IOptions<RefreshTokenOptions> refreshOptions,
        IIdentityAuthService auth,
        HttpContext http,
        CancellationToken ct)
    {
        var cookieName = refreshOptions.Value.CookieName;
        http.Request.Cookies.TryGetValue(cookieName, out var raw);

        var tokens = await auth.RefreshAsync(raw, http.Connection.RemoteIpAddress?.ToString(), ct);
        if (tokens is null)
            return Results.Unauthorized();

        return Results.Ok(new AuthTokenApiResponse(
            tokens.AccessToken,
            tokens.AccessTokenExpiresAtUtc,
            tokens.AccountId,
            tokens.RefreshTokenPlaintext));
    }

    private static async Task<IResult> LogoutAsync(
        IOptions<RefreshTokenOptions> refreshOptions,
        IIdentityAuthService auth,
        HttpContext http,
        CancellationToken ct)
    {
        http.Request.Cookies.TryGetValue(refreshOptions.Value.CookieName, out var raw);
        await auth.LogoutAsync(raw, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest dto,
        IIdentityAuthService auth,
        IConfiguration config,
        CancellationToken ct)
    {
        var baseUrl = config["App:PublicBaseUrl"] ?? "https://localhost";
        await auth.RequestPasswordResetAsync(dto.Email, $"{baseUrl.TrimEnd('/')}/reset-password", ct);
        return Results.Accepted();
    }

    private static async Task<IResult> ResetPasswordAsync(
        [FromBody] ResetPasswordRequest dto,
        IIdentityAuthService auth,
        CancellationToken ct)
    {
        var result = await auth.ResetPasswordAsync(dto.UserId, dto.Code, dto.NewPassword, ct);
        if (!result.Succeeded)
            return Results.ValidationProblem(result.ErrorDictionary());

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAccountAsync(
        [FromBody] DeleteAccountRequest dto,
        UserManager<Account> userManager,
        ClaimsPrincipal principal,
        IIdentityAuthService auth,
        CancellationToken ct)
    {
        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(id, out var userId))
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Results.NotFound();

        var result = await auth.DeleteAccountAsync(user, dto.Password, ct);
        if (!result.Succeeded)
            return Results.ValidationProblem(result.ErrorDictionary());

        auth.ClearRefreshCookie();
        return Results.NoContent();
    }

    private static IResult GoogleChallenge(IConfiguration config)
    {
        var redirect = $"{config["App:PublicBaseUrl"]?.TrimEnd('/')}/api/auth/external/google/complete";
        var props = new AuthenticationProperties { RedirectUri = redirect };
        return Results.Challenge(properties: props, authenticationSchemes: [GoogleDefaults.AuthenticationScheme]);
    }

    private static IResult FacebookChallenge(IConfiguration config)
    {
        var redirect = $"{config["App:PublicBaseUrl"]?.TrimEnd('/')}/api/auth/external/facebook/complete";
        var props = new AuthenticationProperties { RedirectUri = redirect };
        return Results.Challenge(properties: props, authenticationSchemes: [FacebookDefaults.AuthenticationScheme]);
    }

    private static Task<IResult> GoogleCompleteAsync(
        SignInManager<Account> signInManager,
        UserManager<Account> userManager,
        IMessageBus bus,
        IIdentityAuthService auth,
        HttpContext http,
        CancellationToken ct)
        => ExternalCompleteAsync(signInManager, userManager, bus, auth, http, ct);

    private static Task<IResult> FacebookCompleteAsync(
        SignInManager<Account> signInManager,
        UserManager<Account> userManager,
        IMessageBus bus,
        IIdentityAuthService auth,
        HttpContext http,
        CancellationToken ct)
        => ExternalCompleteAsync(signInManager, userManager, bus, auth, http, ct);

    private static async Task<IResult> ExternalCompleteAsync(
        SignInManager<Account> signInManager,
        UserManager<Account> userManager,
        IMessageBus bus,
        IIdentityAuthService auth,
        HttpContext http,
        CancellationToken ct)
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return Results.BadRequest();

        var signIn = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        Account? user;
        if (signIn.Succeeded)
        {
            user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user is null)
                return Results.Unauthorized();
        }
        else
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                return Results.BadRequest();

            user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email.Split('@')[0];
                user = new Account
                {
                    Id = Guid.CreateVersion7(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var randomPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) + "aA1!";
                var created = await userManager.CreateAsync(user, randomPassword);
                if (!created.Succeeded)
                    return Results.ValidationProblem(created.ErrorDictionary());

                var addLogin = await userManager.AddLoginAsync(user, info);
                if (!addLogin.Succeeded)
                {
                    await userManager.DeleteAsync(user);
                    return Results.ValidationProblem(addLogin.ErrorDictionary());
                }

                var role = await userManager.AddToRoleAsync(user, RoleNames.Customer);
                if (!role.Succeeded)
                {
                    await userManager.DeleteAsync(user);
                    return Results.ValidationProblem(role.ErrorDictionary());
                }

                try
                {
                    await bus.InvokeAsync(
                        new ProvisionCustomerForAccountCommand
                        {
                            AccountId = user.Id,
                            Email = email,
                            Name = name,
                            PhoneNumber = info.Principal.FindFirstValue(ClaimTypes.MobilePhone) ?? "—",
                            CorrelationId = http.TraceIdentifier
                        },
                        ct);
                }
                catch
                {
                    await userManager.DeleteAsync(user);
                    throw;
                }
            }
            else
            {
                var addLogin = await userManager.AddLoginAsync(user, info);
                if (!addLogin.Succeeded)
                    return Results.ValidationProblem(addLogin.ErrorDictionary());
            }
        }

        var tokens = await auth.IssueTokensAsync(user, http.Connection.RemoteIpAddress?.ToString(), ct);
        if (tokens is null)
            return Results.Unauthorized();

        return Results.Ok(new AuthTokenApiResponse(
            tokens.AccessToken,
            tokens.AccessTokenExpiresAtUtc,
            tokens.AccountId,
            tokens.RefreshTokenPlaintext));
    }

    private static async Task<IResult> LockAccountAsync(
        Guid accountId,
        [FromBody] LockAccountRequest? body,
        IIdentityAuthService auth,
        CancellationToken ct)
    {
        var end = body?.LockoutEndUtc ?? DateTimeOffset.UtcNow.AddYears(100);
        await auth.LockAccountAsync(accountId, end, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> UnlockAccountAsync(
        Guid accountId,
        IIdentityAuthService auth,
        CancellationToken ct)
    {
        await auth.UnlockAccountAsync(accountId, ct);
        return Results.NoContent();
    }

    private static IDictionary<string, string[]> ErrorDictionary(this IdentityResult result)
    {
        return new Dictionary<string, string[]>
        {
            ["errors"] = result.Errors.Select(e => $"{e.Code}: {e.Description}").ToArray()
        };
    }
}
