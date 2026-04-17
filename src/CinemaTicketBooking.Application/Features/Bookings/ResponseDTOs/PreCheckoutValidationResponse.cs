namespace CinemaTicketBooking.Application.Features;

public record PreCheckoutViolationDto(
    string Type,
    string Severity,
    string Message,
    IReadOnlyList<string> AffectedSeats,
    bool BlockCheckout);

public record PreCheckoutValidationResponse(
    bool CanProceed,
    IReadOnlyList<PreCheckoutViolationDto> Warnings,
    IReadOnlyList<PreCheckoutViolationDto> Errors,
    IReadOnlyList<string> Hints,
    IReadOnlyList<PaymentGatewayOptionDto>? PaymentOptions = null);
