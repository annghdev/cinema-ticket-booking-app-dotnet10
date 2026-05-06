using CinemaTicketBooking.Application.Features;

namespace CinemaTicketBooking.WebServer.Models;

public class DashboardViewModel
{
    public DashboardSummaryDto Summary { get; set; } = null!;
    public RevenueChartDto WeeklyRevenueChart { get; set; } = null!;
    public RevenueChartDto MonthlyRevenueChart { get; set; } = null!;
    public RevenueChartDto YearlyRevenueChart { get; set; } = null!;
    public IReadOnlyList<CinemaRevenueDto> CinemaRevenues { get; set; } = [];
    public IReadOnlyList<MoviePerformanceDto> TopMovies { get; set; } = [];
}
