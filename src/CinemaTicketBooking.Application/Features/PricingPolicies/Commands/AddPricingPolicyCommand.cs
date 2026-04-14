namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Adds a new pricing policy.
/// </summary>
public class AddPricingPolicyCommand : ICommand
{
    public Guid? CinemaId { get; set; }
    public ScreenType ScreenType { get; set; }
    public SeatType SeatType { get; set; }
    public decimal BasePrice { get; set; }
    public decimal ScreenCoefficient { get; set; } = 1m;
    public bool IsActive { get; set; } = true;
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles add-pricing-policy requests.
/// </summary>
public class AddPricingPolicyHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Creates a pricing policy and persists changes.
    /// </summary>
    public async Task<Guid> Handle(AddPricingPolicyCommand cmd, CancellationToken ct)
    {
        var policy = PricingPolicy.Create(
            cinemaId: cmd.CinemaId,
            screenType: cmd.ScreenType,
            seatType: cmd.SeatType,
            basePrice: cmd.BasePrice,
            screenCoefficient: cmd.ScreenCoefficient,
            isActive: cmd.IsActive);

        uow.PricingPolicies.Add(policy);
        await uow.CommitAsync(ct);
        return policy.Id;
    }
}

/// <summary>
/// Validates add-pricing-policy command payload.
/// </summary>
public class AddPricingPolicyValidator : AbstractValidator<AddPricingPolicyCommand>
{
    public AddPricingPolicyValidator()
    {
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
    }
}
