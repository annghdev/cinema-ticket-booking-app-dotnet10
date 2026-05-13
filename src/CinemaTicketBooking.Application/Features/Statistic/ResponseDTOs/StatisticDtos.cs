namespace CinemaTicketBooking.Application.Features;

public record DashboardSummaryDto(
    decimal DailyRevenue,
    int DailyTicketsSold,
    decimal ConcessionRevenue,
    int NowShowingMoviesCount,
    decimal RevenueTrendPercentage,
    decimal TicketTrendPercentage,
    decimal ConcessionTrendPercentage
);

public record RevenueChartDto(
    List<string> Labels,
    List<decimal> Data
);

public record CinemaRevenueDto(
    string CinemaName,
    decimal Revenue,
    decimal Percentage
);

public record MoviePerformanceDto(
    Guid MovieId,
    string Title,
    string ThumbnailUrl,
    string Genre,
    DateTimeOffset ReleaseDate,
    decimal BoxOffice,
    decimal TargetGoal,
    decimal ProgressPercentage,
    bool IsTrendingUp
);
