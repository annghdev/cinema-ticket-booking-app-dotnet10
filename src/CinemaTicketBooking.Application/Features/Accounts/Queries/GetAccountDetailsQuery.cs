using CinemaTicketBooking.Application.Abstractions;

namespace CinemaTicketBooking.Application.Features;

public class GetAccountDetailsQuery(Guid accountId) : IQuery<SystemAccountDetailDto?>
{
    public Guid AccountId { get; set; } = accountId;
    public string CorrelationId { get; set; } = string.Empty;
}

public class GetAccountDetailsHandler(IAuthService auth)
{
    public Task<SystemAccountDetailDto?> Handle(GetAccountDetailsQuery query, CancellationToken ct)
    {
        return auth.GetAccountDetailsAsync(query.AccountId, ct);
    }
}
