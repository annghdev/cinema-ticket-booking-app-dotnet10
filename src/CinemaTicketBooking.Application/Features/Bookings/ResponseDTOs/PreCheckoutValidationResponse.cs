using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Application.Features;

public sealed record PreCheckoutViolationDto(
    string Type,
    string Severity,
    string Message,
    IReadOnlyList<string> AffectedSeats,
    bool BlockCheckout);

public sealed record PreCheckoutValidationResponse(
    bool CanProceed,
    IReadOnlyList<PreCheckoutViolationDto> Warnings,
    IReadOnlyList<PreCheckoutViolationDto> Errors,
    IReadOnlyList<string> Hints,
    IReadOnlyList<PaymentGatewayOptionDto>? PaymentOptions = null);
