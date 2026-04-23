using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class PaymentTransactionRepository(AppDbContext db)
    : BaseRepository<PaymentTransaction>(db), IPaymentTransactionRepository
{
    /// <summary>
    /// Returns the most recent pending payment transaction for the given booking.
    /// </summary>
    public async Task<PaymentTransaction?> GetPendingByBookingIdAsync(
        Guid bookingId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.BookingId == bookingId && x.Status == PaymentTransactionStatus.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Returns the most recent transaction by gateway transaction id.
    /// </summary>
    public async Task<PaymentTransaction?> GetByGatewayTransactionIdAsync(
        string gatewayTransactionId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.GatewayTransactionId == gatewayTransactionId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }
}
