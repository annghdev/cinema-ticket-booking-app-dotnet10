using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Application.Common.Auth;

/// <summary>
/// Authorization helpers for resource ownership vs admin permissions.
/// </summary>
public static class PermissionHelper
{
    /// <summary>
    /// Ensures the user may access a booking: same customer, or admin with view-all permission, or global Admin role.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">When access is denied.</exception>
    public static void EnsureCanAccessBooking(IUserContext user, Booking booking)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(booking);

        if (user.HasPermission(Permissions.BookingsViewAll) || user.IsInRole(RoleNames.Admin))
            return;

        if (!user.CustomerId.HasValue || booking.CustomerId != user.CustomerId)
            throw new UnauthorizedAccessException("You are not allowed to access this booking.");
    }

    /// <summary>
    /// Ensures the user may access a booking by customer id (when booking aggregate is not loaded).
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">When access is denied.</exception>
    public static void EnsureCanAccessBooking(IUserContext user, Guid? bookingCustomerId)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.HasPermission(Permissions.BookingsViewAll) || user.IsInRole(RoleNames.Admin))
            return;

        if (!user.CustomerId.HasValue || bookingCustomerId != user.CustomerId)
            throw new UnauthorizedAccessException("You are not allowed to access this booking.");
    }
}
