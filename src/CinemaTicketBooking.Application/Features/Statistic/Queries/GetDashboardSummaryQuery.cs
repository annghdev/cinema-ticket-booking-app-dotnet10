using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Application.Features;

namespace CinemaTicketBooking.Application.Features.Statistic.Queries;

public record GetDashboardSummaryQuery : IQuery<DashboardSummaryDto>
{
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public class GetDashboardSummaryHandler(IStatisticService statisticService)
{
    public Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery query, CancellationToken ct)
    {
        return statisticService.GetDashboardSummaryAsync(ct);
    }
}
