using System.Text.Json.Serialization;

namespace CinemaTicketBooking.Domain;

/// <summary>
/// Tracks a single payment attempt for a Booking.
/// Immutable transaction log entry — extends DefaultEntity (no soft-delete or audit).
/// Each Booking may have multiple PaymentTransactions (e.g., retry after failure).
/// </summary>
public class PaymentTransaction : DefaultEntity
{
    public Guid BookingId { get; set; }

    [JsonIgnore]
    public Booking? Booking { get; set; }

    /// <summary>
    /// The gateway used for this payment attempt.
    /// </summary>
    public PaymentMethod Method { get; set; }

    /// <summary>
    /// Transaction reference from the payment gateway (e.g., VnPay TxnRef, Momo RequestId).
    /// </summary>
    public string GatewayTransactionId { get; set; } = string.Empty;

    /// <summary>
    /// How the client should handle payment presentation.
    /// </summary>
    public PaymentRedirectBehavior RedirectBehavior { get; set; }

    /// <summary>
    /// Payment URL for Redirect behavior, or QR code data/URL for QrCode behavior.
    /// </summary>
    public string PaymentUrl { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public PaymentTransactionStatus Status { get; set; }

    /// <summary>
    /// Optional raw response from the gateway for debugging.
    /// </summary>
    public string? GatewayResponseRaw { get; set; }

    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
