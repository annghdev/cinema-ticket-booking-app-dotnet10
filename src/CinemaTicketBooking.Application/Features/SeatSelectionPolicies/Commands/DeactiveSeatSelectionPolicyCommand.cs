namespace CinemaTicketBooking.Application.Features;

public class DeactiveSeatSelectionPolicyCommand : ICommand
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

public class DeactiveSeatSelectionPolicyHandler(IUnitOfWork uow)
{
    public async Task Handle(DeactiveSeatSelectionPolicyCommand cmd, CancellationToken ct)
    {
        var policy = await uow.SeatSelectionPolicies.GetByIdAsync(cmd.Id, ct);
        if (policy is null)
        {
            throw new InvalidOperationException(
                $"Seat selection policy with ID '{cmd.Id}' not found.");
        }

        policy.IsActive = false;
        uow.SeatSelectionPolicies.Update(policy);
        await uow.CommitAsync(ct);
    }
}