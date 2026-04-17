namespace CinemaTicketBooking.Application.Common.Auth;

/// <summary>
/// JWT and identity claim type constants used across layers.
/// </summary>
public static class AuthClaimTypes
{
    public const string Permission = "permission";

    /// <summary>
    /// Registered customer profile id (domain <see cref="Domain.Customer"/>).
    /// </summary>
    public const string CustomerId = "customer_id";
}
