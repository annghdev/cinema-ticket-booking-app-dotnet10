namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Validates selected tickets against seat-layout policy before checkout can proceed.
/// </summary>
public class ValidateSeatSelectionCommand : ICommand
{
    public Guid ShowTimeId { get; set; }
    public List<Guid> SelectedTicketIds { get; set; } = [];
    public string CustomerSessionId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles pre-checkout seat-layout validation.
/// </summary>
public class ValidateSeatSelectionHandler(
    IUnitOfWork uow,
    IPaymentServiceFactory paymentServiceFactory)
{
    /// <summary>
    /// Loads seat-layout context and returns warning/error details for checkout UI.
    /// </summary>
    public async Task<ValidateSeatSelectionResponse> Handle(
        ValidateSeatSelectionCommand command,
        CancellationToken ct)
    {
        // 1. Load full showtime aggregate for seat-layout evaluation.
        var showTime = await uow.ShowTimes.LoadFullAsync(command.ShowTimeId, ct)
            ?? throw new InvalidOperationException($"ShowTime with ID '{command.ShowTimeId}' was not found.");

        if (showTime.Status == ShowTimeStatus.Cancelled)
        {
            throw new InvalidOperationException("ShowTime is cancelled. Cannot select seats.");
        }

        // 2. Resolve active global policy, fallback to default in-memory policy.
        var policy = await uow.SeatSelectionPolicies.GetActiveGlobalAsync(ct)
            ?? SeatSelectionPolicy.CreateDefault();

        // 3. Execute domain validator.
        var domainValidator = SeatSelectionValidator.CreateDefault();
        var validationResult = domainValidator.Validate(
            showTime,
            policy,
            command.SelectedTicketIds,
            command.CustomerSessionId);

        // 4. Map domain result to API-facing DTO, include payment options when checkout can proceed.
        return new ValidateSeatSelectionResponse(
            CanProceed: validationResult.CanProceed,
            Warnings: validationResult.Warnings.Select(ToViolationDto).ToList(),
            Errors: validationResult.Errors.Select(ToViolationDto).ToList(),
            Hints: validationResult.Hints,
            PaymentOptions: validationResult.CanProceed
                ? paymentServiceFactory.GetAvailableOptions()
                : null);
    }

    private static SeatSelectionViolationDto ToViolationDto(SeatSelectionViolation violation)
    {
        return new SeatSelectionViolationDto(
            Type: ToSnakeUpper(violation.Type.ToString()),
            Severity: violation.BlockCheckout ? "error" : "warning",
            Message: violation.Message,
            AffectedSeats: violation.AffectedSeats,
            BlockCheckout: violation.BlockCheckout);
    }

    private static string ToSnakeUpper(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var chars = new List<char>(value.Length * 2);
        for (var index = 0; index < value.Length; index++)
        {
            var c = value[index];
            if (index > 0 && char.IsUpper(c) && char.IsLower(value[index - 1]))
            {
                chars.Add('_');
            }

            chars.Add(char.ToUpperInvariant(c));
        }

        return new string(chars.ToArray());
    }
}

/// <summary>
/// Validates pre-checkout command payload.
/// </summary>
public class ValidateSeatSelectionValidator : AbstractValidator<ValidateSeatSelectionCommand>
{
    public ValidateSeatSelectionValidator()
    {
        RuleFor(x => x.ShowTimeId)
            .NotEmpty()
            .WithMessage("ShowTime ID is required.");

        RuleFor(x => x.CustomerSessionId)
            .NotEmpty()
            .WithMessage("Customer session ID is required.")
            .MaximumLength(MaxLengthConsts.SessionId);

        RuleFor(x => x.SelectedTicketIds)
            .NotEmpty()
            .WithMessage("Selected ticket IDs are required.");
    }
}
