namespace CinemaTicketBooking.Application.Features;

public sealed record CreateBookingResponse(
    Guid BookingId,
    DateTimeOffset PaymentExpiresAt,
    decimal OriginAmount,
    decimal FinalAmount,
    string PaymentStatus,
    string? PaymentUrl = null,
    PaymentRedirectBehavior? RedirectBehavior = null,
    Guid? PaymentTransactionId = null);
