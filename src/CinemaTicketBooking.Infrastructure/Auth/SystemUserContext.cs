using CinemaTicketBooking.Application;

namespace CinemaTicketBooking.Infrastructure.Auth;

/// <summary>
/// Fallback user context for non-HTTP workloads (background handlers, integration host without HTTP).
/// </summary>
public sealed class SystemUserContext : IUserContext
{
    /// <inheritdoc />
    public bool IsAuthenticated => false;

    /// <inheritdoc />
    public Guid UserId => Guid.Empty;

    /// <inheritdoc />
    public string UserName => "system";

    /// <inheritdoc />
    public bool IsInRole(string role) => false;

    /// <inheritdoc />
    public Guid? CustomerId => null;

    /// <inheritdoc />
    public IReadOnlySet<string> Permissions => new HashSet<string>(StringComparer.Ordinal);

    /// <inheritdoc />
    public bool HasPermission(string permission) => false;
}
