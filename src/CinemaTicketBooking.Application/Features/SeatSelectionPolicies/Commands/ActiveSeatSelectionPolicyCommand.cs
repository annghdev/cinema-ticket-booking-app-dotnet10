namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Sets seat selection policy state to active/inactive.
/// </summary>
public class ActiveSeatSelectionPolicyCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles set-seat-selection-policy-activation requests.
/// </summary>
public class ActiveSeatSelectionPolicyHandler(IUnitOfWork uow)
{
    public async Task Handle(ActiveSeatSelectionPolicyCommand cmd, CancellationToken ct)
    {
        var policy = await uow.SeatSelectionPolicies.GetByIdAsync(cmd.Id, ct);
        if (policy is null)
        {
            throw new InvalidOperationException(
                $"Seat selection policy with ID '{cmd.Id}' not found.");
        }

        policy.IsActive = true;
        uow.SeatSelectionPolicies.Update(policy);
        await uow.CommitAsync(ct);
    }
}

public class ActiveSeatSelectionPolicyValidator : AbstractValidator<ActiveSeatSelectionPolicyCommand>
{
    public ActiveSeatSelectionPolicyValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("ID is required.");
    }
}
