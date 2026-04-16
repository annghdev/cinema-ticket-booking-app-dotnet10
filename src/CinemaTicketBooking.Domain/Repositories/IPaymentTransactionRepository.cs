namespace CinemaTicketBooking.Domain;

/// <summary>
/// Repository interface for PaymentTransaction.
/// Extends IRepository with queries for pending transactions by booking.
/// </summary>
public interface IPaymentTransactionRepository : IRepository<PaymentTransaction>
{
    /// <summary>
    /// Returns the most recent pending payment transaction for the given booking.
    /// Returns null if no pending transaction exists.
    /// </summary>
    Task<PaymentTransaction?> GetPendingByBookingIdAsync(Guid bookingId, CancellationToken ct = default);
}
