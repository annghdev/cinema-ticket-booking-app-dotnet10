namespace CinemaTicketBooking.Application.Common.Auth;

/// <summary>
/// Static permission codes stored in <c>AspNetRoleClaims</c> (claim type <see cref="AuthClaimTypes.Permission"/>).
/// </summary>
public static class Permissions
{
    public const string BookingsViewAll = "bookings.view_all";

    public const string AccountsLock = "accounts.lock";
    public const string AccountsUnlock = "accounts.unlock";
}
