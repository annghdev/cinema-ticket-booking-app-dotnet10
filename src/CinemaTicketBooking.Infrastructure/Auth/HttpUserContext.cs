using System.Security.Claims;
using CinemaTicketBooking.Application;
using CinemaTicketBooking.Application.Common.Auth;
using Microsoft.AspNetCore.Http;

namespace CinemaTicketBooking.Infrastructure.Auth;

/// <summary>
/// Resolves <see cref="IUserContext"/> from the current HTTP user principal.
/// </summary>
public sealed class HttpUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    /// <inheritdoc />
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc />
    public Guid UserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : Guid.Empty;

    /// <inheritdoc />
    public string UserName =>
        User?.FindFirstValue(ClaimTypes.Name)
        ?? User?.Identity?.Name
        ?? string.Empty;

    /// <inheritdoc />
    public bool IsInRole(string role) =>
        User?.IsInRole(role) ?? false;

    /// <inheritdoc />
    public Guid? CustomerId
    {
        get
        {
            var raw = User?.FindFirstValue(AuthClaimTypes.CustomerId);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    /// <inheritdoc />
    public IReadOnlySet<string> Permissions =>
        User?.FindAll(AuthClaimTypes.Permission).Select(c => c.Value).ToHashSet(StringComparer.Ordinal)
        ?? new HashSet<string>(StringComparer.Ordinal);

    /// <inheritdoc />
    public bool HasPermission(string permission) =>
        Permissions.Contains(permission);
}
