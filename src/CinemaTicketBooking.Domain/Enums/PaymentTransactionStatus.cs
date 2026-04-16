namespace CinemaTicketBooking.Domain;

/// <summary>
/// Lifecycle states of a payment transaction.
/// </summary>
public enum PaymentTransactionStatus
{
    Pending,
    Success,
    Failed,
    Expired,
    Cancelled
}
