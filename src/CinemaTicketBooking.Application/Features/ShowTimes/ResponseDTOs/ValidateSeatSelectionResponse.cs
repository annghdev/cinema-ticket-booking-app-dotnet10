namespace CinemaTicketBooking.Application.Features;

public record SeatSelectionViolationDto(
    string Type,
    string Severity,
    string Message,
    IReadOnlyList<string> AffectedSeats,
    bool BlockCheckout);

public record ValidateSeatSelectionResponse(
    bool CanProceed,
    IReadOnlyList<SeatSelectionViolationDto> Warnings,
    IReadOnlyList<SeatSelectionViolationDto> Errors,
    IReadOnlyList<string> Hints,
    IReadOnlyList<PaymentGatewayOptionDto>? PaymentOptions = null);
