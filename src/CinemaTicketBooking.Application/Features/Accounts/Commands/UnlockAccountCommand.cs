using CinemaTicketBooking.Application.Abstractions;

namespace CinemaTicketBooking.Application.Features;

public class UnlockAccountCommand : ICommand
{
    public Guid AccountId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

public class UnlockAccountHandler(IAuthService auth)
{
    public async Task Handle(UnlockAccountCommand cmd, CancellationToken ct)
    {
        await auth.UnlockAccountAsync(cmd.AccountId, ct);
    }
}
