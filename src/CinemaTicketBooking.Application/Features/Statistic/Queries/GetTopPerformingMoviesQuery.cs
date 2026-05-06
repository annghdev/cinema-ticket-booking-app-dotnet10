using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Application.Features;

namespace CinemaTicketBooking.Application.Features.Statistic.Queries;

public record GetTopPerformingMoviesQuery : IQuery<IReadOnlyList<MoviePerformanceDto>>
{
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public class GetTopPerformingMoviesHandler(IStatisticService statisticService)
{
    public Task<IReadOnlyList<MoviePerformanceDto>> Handle(GetTopPerformingMoviesQuery query, CancellationToken ct)
    {
        return statisticService.GetTopPerformingMoviesAsync(ct);
    }
}
