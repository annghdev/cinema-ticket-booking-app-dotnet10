namespace CinemaTicketBooking.Application.Abstractions;

/// <summary>
/// Links an Identity <c>Account</c> to a domain <see cref="Domain.Customer"/> profile.
/// </summary>
public interface IAccountCustomerLinker
{
    /// <summary>
    /// Sets <c>Account.CustomerId</c> for the given account.
    /// </summary>
    Task SetCustomerIdAsync(Guid accountId, Guid customerId, CancellationToken cancellationToken = default);
}
