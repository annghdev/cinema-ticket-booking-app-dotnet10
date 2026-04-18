using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Gets showtimes with pagination, filtering, and sorting.
/// </summary>
public class GetPagedShowTimesQuery : IQuery<PagedResult<ShowTimeDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CinemaId { get; set; }
    public Guid? MovieId { get; set; }
    public Guid? ScreenId { get; set; }
    public ShowTimeStatus? Status { get; set; }
    public DateOnly? Date { get; set; }
    public string SortBy { get; set; } = "startat";
    public string SortDirection { get; set; } = "asc";
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles query for paged showtime list.
/// </summary>
public class GetPagedShowTimesHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Returns paged showtimes after applying filter and sorting.
    /// </summary>
    public async Task<PagedResult<ShowTimeDto>> Handle(GetPagedShowTimesQuery query, CancellationToken ct)
    {
        var dbQuery = uow.ShowTimes.GetQueryFilter();
        dbQuery = ApplyFilter(dbQuery, query);
        dbQuery = ApplySorting(dbQuery, query);

        var totalItems = await dbQuery.CountAsync(ct);
        var skip = (query.PageNumber - 1) * query.PageSize;

        var items = await dbQuery
            .Skip(skip)
            .Take(query.PageSize)
            .Select(x => new ShowTimeDto(
                x.Id,
                x.MovieId,
                x.Movie != null ? x.Movie.Name : string.Empty,
                x.ScreenId,
                x.Screen != null ? x.Screen.Code : string.Empty,
                x.Screen != null ? x.Screen.CinemaId : Guid.Empty,
                x.Screen != null && x.Screen.Cinema != null ? x.Screen.Cinema.Name : string.Empty,
                x.Date,
                x.StartAt,
                x.EndAt,
                x.Status,
                x.Tickets.Count,
                // Only Available is bookable; Locking/PendingPayment/Sold are unavailable.
                x.Tickets.Count(t => t.Status == TicketStatus.Available),
                x.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<ShowTimeDto>(items, totalItems, query.PageNumber, query.PageSize);
    }

    private static IQueryable<ShowTime> ApplyFilter(IQueryable<ShowTime> dbQuery, GetPagedShowTimesQuery query)
    {
        if (query.CinemaId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Screen != null && x.Screen.CinemaId == query.CinemaId.Value);
        }

        if (query.MovieId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.MovieId == query.MovieId.Value);
        }

        if (query.ScreenId.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.ScreenId == query.ScreenId.Value);
        }

        if (query.Status.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Status == query.Status.Value);
        }

        if (query.Date.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Date == query.Date.Value);
        }

        return dbQuery;
    }

    private static IQueryable<ShowTime> ApplySorting(IQueryable<ShowTime> dbQuery, GetPagedShowTimesQuery query)
    {
        var sortBy = query.SortBy.Trim().ToLowerInvariant();
        var isDesc = query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return (sortBy, isDesc) switch
        {
            ("date", true) => dbQuery.OrderByDescending(x => x.Date),
            ("date", false) => dbQuery.OrderBy(x => x.Date),

            ("startat", true) => dbQuery.OrderByDescending(x => x.StartAt),
            ("startat", false) => dbQuery.OrderBy(x => x.StartAt),

            ("endat", true) => dbQuery.OrderByDescending(x => x.EndAt),
            ("endat", false) => dbQuery.OrderBy(x => x.EndAt),

            ("status", true) => dbQuery.OrderByDescending(x => x.Status),
            ("status", false) => dbQuery.OrderBy(x => x.Status),

            ("createdat", true) => dbQuery.OrderByDescending(x => x.CreatedAt),
            ("createdat", false) => dbQuery.OrderBy(x => x.CreatedAt),

            (_, true) => dbQuery.OrderByDescending(x => x.StartAt),
            _ => dbQuery.OrderBy(x => x.StartAt)
        };
    }
}

/// <summary>
/// Validates paged showtime query payload.
/// </summary>
public class GetPagedShowTimesValidator : AbstractValidator<GetPagedShowTimesQuery>
{
    private static readonly string[] SupportedSortBy = ["date", "startat", "endat", "status", "createdat"];
    private static readonly string[] SupportedSortDirections = ["asc", "desc"];

    public GetPagedShowTimesValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.CinemaId)
            .Must(x => !x.HasValue || x.Value != Guid.Empty)
            .WithMessage("Cinema ID is invalid.");

        RuleFor(x => x.MovieId)
            .Must(x => !x.HasValue || x.Value != Guid.Empty)
            .WithMessage("Movie ID is invalid.");

        RuleFor(x => x.ScreenId)
            .Must(x => !x.HasValue || x.Value != Guid.Empty)
            .WithMessage("Screen ID is invalid.");

        RuleFor(x => x.Status)
            .Must(status => status is null || Enum.IsDefined(status.Value))
            .WithMessage("Invalid showtime status.");

        RuleFor(x => x.SortBy)
            .NotEmpty().WithMessage("SortBy is required.")
            .Must(sortBy => SupportedSortBy.Contains(sortBy.Trim().ToLowerInvariant()))
            .WithMessage($"SortBy is invalid. Supported values: {string.Join(", ", SupportedSortBy)}.");

        RuleFor(x => x.SortDirection)
            .NotEmpty().WithMessage("SortDirection is required.")
            .Must(direction => SupportedSortDirections.Contains(direction.Trim().ToLowerInvariant()))
            .WithMessage("SortDirection is invalid. Supported values: asc, desc.");
    }
}
