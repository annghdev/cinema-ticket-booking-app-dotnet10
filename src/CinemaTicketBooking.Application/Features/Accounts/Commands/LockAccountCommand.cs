using CinemaTicketBooking.Application.Abstractions;

namespace CinemaTicketBooking.Application.Features;

public class LockAccountCommand : ICommand
{
    public Guid AccountId { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

public class LockAccountHandler(IAuthService auth)
{
    public async Task Handle(LockAccountCommand cmd, CancellationToken ct)
    {
        var end = cmd.LockoutEnd ?? DateTimeOffset.UtcNow.AddYears(100);
        await auth.LockAccountAsync(cmd.AccountId, end, ct);
    }
}
