namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Sets pricing policy state to active.
/// </summary>
public class SetPricingPolicyActiveCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles set-pricing-policy-active requests.
/// </summary>
public class SetPricingPolicyActiveHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Marks the target pricing policy as active and persists changes.
    /// </summary>
    public async Task Handle(SetPricingPolicyActiveCommand cmd, CancellationToken ct)
    {
        var policy = await uow.PricingPolicies.GetByIdAsync(cmd.Id, ct);
        if (policy is null)
        {
            throw new InvalidOperationException($"Pricing policy with ID '{cmd.Id}' not found.");
        }

        policy.Activate();
        uow.PricingPolicies.Update(policy);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Sets pricing policy state to inactive.
/// </summary>
public class SetPricingPolicyInactiveCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles set-pricing-policy-inactive requests.
/// </summary>
public class SetPricingPolicyInactiveHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Marks the target pricing policy as inactive and persists changes.
    /// </summary>
    public async Task Handle(SetPricingPolicyInactiveCommand cmd, CancellationToken ct)
    {
        var policy = await uow.PricingPolicies.GetByIdAsync(cmd.Id, ct);
        if (policy is null)
        {
            throw new InvalidOperationException($"Pricing policy with ID '{cmd.Id}' not found.");
        }

        policy.Deactivate();
        uow.PricingPolicies.Update(policy);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates set-pricing-policy-active command payload.
/// </summary>
public class SetPricingPolicyActiveValidator : AbstractValidator<SetPricingPolicyActiveCommand>
{
    public SetPricingPolicyActiveValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Pricing policy ID is required.");
    }
}

/// <summary>
/// Validates set-pricing-policy-inactive command payload.
/// </summary>
public class SetPricingPolicyInactiveValidator : AbstractValidator<SetPricingPolicyInactiveCommand>
{
    public SetPricingPolicyInactiveValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Pricing policy ID is required.");
    }
}
