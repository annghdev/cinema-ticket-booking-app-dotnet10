namespace CinemaTicketBooking.Application.Features;

public sealed record CreateBookingAndProcessPaymentResponse(
    Guid BookingId,
    DateTimeOffset PaymentExpiresAt,
    decimal OriginAmount,
    decimal FinalAmount,
    string PaymentStatus);
