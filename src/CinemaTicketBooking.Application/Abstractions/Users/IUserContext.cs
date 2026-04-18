namespace CinemaTicketBooking.Application;

/// <summary>
/// Current authenticated user context for auditing and authorization in application handlers.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// True when the request has an authenticated user (JWT or other scheme).
    /// </summary>
    bool IsAuthenticated { get; }

    Guid UserId { get; }
    string UserName { get; }
    bool IsInRole(string role);

    /// <summary>
    /// Linked domain customer id when the account is provisioned; otherwise null.
    /// </summary>
    Guid? CustomerId { get; }

    /// <summary>
    /// Permission codes from role claims (and optional user claims).
    /// </summary>
    IReadOnlySet<string> Permissions { get; }

    /// <summary>
    /// Returns whether the user has the given permission claim.
    /// </summary>
    bool HasPermission(string permission);
}

