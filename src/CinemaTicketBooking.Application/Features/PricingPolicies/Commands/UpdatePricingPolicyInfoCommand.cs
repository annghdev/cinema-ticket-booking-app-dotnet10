namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Updates basic information for an existing pricing policy.
/// </summary>
public class UpdatePricingPolicyInfoCommand : ICommand
{
    public Guid Id { get; set; }
    public Guid? CinemaId { get; set; }
    public ScreenType ScreenType { get; set; }
    public SeatType SeatType { get; set; }
    public decimal BasePrice { get; set; }
    public decimal ScreenCoefficient { get; set; } = 1m;
    public decimal WeekendCoefficient { get; set; } = 1m;
    public bool IsActive { get; set; } = true;
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles pricing policy basic-info update requests.
/// </summary>
public class UpdatePricingPolicyInfoHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Updates basic information for an existing pricing policy and persists changes.
    /// </summary>
    public async Task Handle(UpdatePricingPolicyInfoCommand cmd, CancellationToken ct)
    {
        var policy = await uow.PricingPolicies.GetByIdAsync(cmd.Id, ct);
        if (policy is null)
        {
            throw new InvalidOperationException($"Pricing policy with ID '{cmd.Id}' not found.");
        }

        policy.UpdateBasicInfo(
            cinemaId: cmd.CinemaId,
            screenType: cmd.ScreenType,
            seatType: cmd.SeatType,
            basePrice: cmd.BasePrice,
            screenCoefficient: cmd.ScreenCoefficient,
            weekendCoefficient: cmd.WeekendCoefficient,
            isActive: cmd.IsActive);

        uow.PricingPolicies.Update(policy);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates pricing policy basic-info update command payload.
/// </summary>
public class UpdatePricingPolicyInfoValidator : AbstractValidator<UpdatePricingPolicyInfoCommand>
{
    public UpdatePricingPolicyInfoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Pricing policy ID is required.");

        RuleFor(x => x.CinemaId)
            .Must(cinemaId => !cinemaId.HasValue || cinemaId.Value != Guid.Empty)
            .WithMessage("Cinema ID is invalid.");

        RuleFor(x => x.ScreenType)
            .IsInEnum().WithMessage("Invalid screen type.");

        RuleFor(x => x.SeatType)
            .IsInEnum().WithMessage("Invalid seat type.");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Base price cannot be negative.");

        RuleFor(x => x.ScreenCoefficient)
            .GreaterThan(0).WithMessage("Screen coefficient must be positive.");

        RuleFor(x => x.WeekendCoefficient)
            .GreaterThan(0).WithMessage("Weekend coefficient must be positive.");
    }
}
