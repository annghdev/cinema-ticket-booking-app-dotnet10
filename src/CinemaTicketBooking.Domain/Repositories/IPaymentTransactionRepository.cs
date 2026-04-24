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

    /// <summary>
    /// Returns all pending payment transactions for the given booking.
    /// </summary>
    Task<List<PaymentTransaction>> GetAllPendingByBookingIdAsync(Guid bookingId, CancellationToken ct = default);

    /// <summary>
    /// Returns the most recent transaction by gateway transaction id.
    /// Returns null if transaction does not exist.
    /// </summary>
    Task<PaymentTransaction?> GetByGatewayTransactionIdAsync(string gatewayTransactionId, CancellationToken ct = default);
}
