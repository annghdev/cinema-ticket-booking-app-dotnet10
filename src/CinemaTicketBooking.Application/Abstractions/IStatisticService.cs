using CinemaTicketBooking.Application.Features;

namespace CinemaTicketBooking.Application.Abstractions;

public interface IStatisticService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default);
    Task<RevenueChartDto> GetRevenueAnalyticsAsync(string period, CancellationToken ct = default);
    Task<IReadOnlyList<CinemaRevenueDto>> GetCinemaRevenueStatsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MoviePerformanceDto>> GetTopPerformingMoviesAsync(CancellationToken ct = default);
}
