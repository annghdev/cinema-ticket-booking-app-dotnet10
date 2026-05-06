using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Application.Features;

namespace CinemaTicketBooking.Application.Features.Statistic.Queries;

public record GetCinemaRevenueStatsQuery : IQuery<IReadOnlyList<CinemaRevenueDto>>
{
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public class GetCinemaRevenueStatsHandler(IStatisticService statisticService)
{
    public Task<IReadOnlyList<CinemaRevenueDto>> Handle(GetCinemaRevenueStatsQuery query, CancellationToken ct)
    {
        return statisticService.GetCinemaRevenueStatsAsync(ct);
    }
}
