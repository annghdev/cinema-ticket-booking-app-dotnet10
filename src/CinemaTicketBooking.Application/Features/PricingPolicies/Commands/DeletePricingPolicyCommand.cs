namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Deletes an existing pricing policy.
/// </summary>
public class DeletePricingPolicyCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles pricing-policy deletion requests.
/// </summary>
public class DeletePricingPolicyHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Deletes the target pricing policy and persists changes.
    /// </summary>
    public async Task Handle(DeletePricingPolicyCommand cmd, CancellationToken ct)
    {
        var policy = await uow.PricingPolicies.GetByIdAsync(cmd.Id, ct);
        if (policy is null)
        {
            throw new InvalidOperationException($"Pricing policy with ID '{cmd.Id}' not found.");
        }

        policy.MarkAsDeleted();
        uow.PricingPolicies.Delete(policy);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates delete-pricing-policy command payload.
/// </summary>
public class DeletePricingPolicyValidator : AbstractValidator<DeletePricingPolicyCommand>
{
    public DeletePricingPolicyValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Pricing policy ID is required.");
    }
}
