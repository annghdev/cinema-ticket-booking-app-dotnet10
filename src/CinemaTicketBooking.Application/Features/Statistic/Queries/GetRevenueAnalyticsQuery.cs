using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Application.Features;

namespace CinemaTicketBooking.Application.Features.Statistic.Queries;

public record GetRevenueAnalyticsQuery(string Period = "monthly") : IQuery<RevenueChartDto>
{
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public class GetRevenueAnalyticsHandler(IStatisticService statisticService)
{
    public Task<RevenueChartDto> Handle(GetRevenueAnalyticsQuery query, CancellationToken ct)
    {
        return statisticService.GetRevenueAnalyticsAsync(query.Period, ct);
    }
}
