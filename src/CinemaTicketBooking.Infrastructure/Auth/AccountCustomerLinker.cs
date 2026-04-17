using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Infrastructure.Auth;

/// <summary>
/// Updates <see cref="Account.CustomerId"/> on the Identity account row.
/// </summary>
public sealed class AccountCustomerLinker(AppDbContext db) : IAccountCustomerLinker
{
    /// <inheritdoc />
    public async Task SetCustomerIdAsync(Guid accountId, Guid customerId, CancellationToken cancellationToken = default)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(x => x.Id == accountId, cancellationToken)
            ?? throw new InvalidOperationException($"Account '{accountId}' was not found.");

        account.CustomerId = customerId;
        // await db.SaveChangesAsync(cancellationToken);
    }
}
