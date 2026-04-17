namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Response returned after verifying a payment gateway callback.
/// If the payment failed, CanRetry is true, indicating the client should
/// allow the user to select a different payment gateway and retry.
/// </summary>
public record VerifyPaymentResponse(
    Guid BookingId,
    Guid PaymentTransactionId,
    bool IsSuccess,
    string? CheckinQrCode,
    string Status,
    string? ErrorMessage = null,
    bool CanRetry = false,
    IReadOnlyList<PaymentGatewayOptionDto>? AvailableGateways = null);
