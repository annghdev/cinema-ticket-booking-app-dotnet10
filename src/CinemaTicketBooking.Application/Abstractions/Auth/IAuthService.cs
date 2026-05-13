using Microsoft.AspNetCore.Identity;
using CinemaTicketBooking.Application.Features;

namespace CinemaTicketBooking.Application.Abstractions;

/// <summary>
/// Identity-backed authentication operations (tokens, refresh cookies, password, lockout).
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Issues access + refresh tokens and appends the refresh cookie when <see cref="Microsoft.AspNetCore.Http.HttpContext"/> is available.
    /// </summary>
    Task<AuthTokenResponse?> IssueTokensAsync(Guid accountId, string? remoteIp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates refresh cookie, rotates refresh, returns new access token.
    /// </summary>
    Task<AuthTokenResponse?> RefreshAsync(string? refreshTokenRaw, string? remoteIp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes the refresh token matching the cookie and deletes the cookie.
    /// </summary>
    Task LogoutAsync(string? refreshTokenRaw, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes every active refresh session for the account (security lock, hack suspicion).
    /// </summary>
    Task RevokeAllRefreshTokensAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends reset email when the account exists.
    /// </summary>
    Task RequestPasswordResetAsync(string email, string resetPageBaseUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets password from emailed token.
    /// </summary>
    Task<IdentityResult> ResetPasswordAsync(Guid userId, string resetCode, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-removes the user after password confirmation and revokes refresh tokens.
    /// </summary>
    Task<IdentityResult> DeleteAccountAsync(Guid accountId, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Locks the account until <paramref name="lockoutEnd"/> (UTC) and revokes refresh tokens.
    /// </summary>
    Task LockAccountAsync(Guid accountId, DateTimeOffset lockoutEnd, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears lockout for the account.
    /// </summary>
    Task UnlockAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new system account with specified roles.
    /// </summary>
    Task<IdentityResult> CreateAccountAsync(string email, string userName, string password, List<string> roles, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches full account details including roles and specific permission claims.
    /// </summary>
    Task<SystemAccountDetailDto?> GetAccountDetailsAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// Synchronizes account roles and custom permission claims.
    /// </summary>
    Task<IdentityResult> UpdateAccountRolesAndClaimsAsync(Guid accountId, List<string> roles, List<string> permissions, CancellationToken ct = default);

    /// <summary>
    /// Force-resets the password to a randomly generated secure string and returns it.
    /// </summary>
    Task<string> AdminResetPasswordAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// Clears the refresh token cookie on the response.
    /// </summary>
    void ClearRefreshCookie();
}

/// <summary>
/// API-facing access token payload.
/// </summary>
public sealed record AuthTokenResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    Guid AccountId,
    string? RefreshTokenPlaintext = null);
