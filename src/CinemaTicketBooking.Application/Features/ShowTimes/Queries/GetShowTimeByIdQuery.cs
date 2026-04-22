using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Gets a showtime by id.
/// </summary>
public class GetShowTimeByIdQuery : IQuery<ShowTimeDetailDto?>
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles query for a specific showtime.
/// </summary>
public class GetShowTimeByIdHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Returns showtime data when found; otherwise null.
    /// </summary>
    public async Task<ShowTimeDetailDto?> Handle(GetShowTimeByIdQuery query, CancellationToken ct)
    {
        var showTime = await uow.ShowTimes
            .GetQueryFilter()
            .Include(x => x.Movie)
            .Include(x => x.Screen)
                .ThenInclude(x => x!.Cinema)
            .Include(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == query.Id, ct);

        if (showTime is null)
        {
            return null;
        }

        var tickets = showTime.Tickets
            .OrderBy(x => x.Code)
            .Select(x => new ShowTimeTicketDto(
                x.Id,
                x.Code,
                x.Price,
                x.Status,
                x.LockingBy))
            .ToList();

        var availableTicketCount = showTime.Tickets.Count(x => x.Status == TicketStatus.Available);

        return new ShowTimeDetailDto(
            showTime.Id,
            showTime.MovieId,
            showTime.Movie?.Name ?? string.Empty,
            showTime.ScreenId,
            showTime.Screen?.Code ?? string.Empty,
            showTime.Screen?.CinemaId ?? Guid.Empty,
            showTime.Screen?.Cinema?.Name ?? string.Empty,
            showTime.Screen?.SeatMap ?? string.Empty,
            showTime.Date,
            showTime.StartAt,
            showTime.EndAt,
            showTime.Status,
            showTime.Tickets.Count,
            availableTicketCount,
            showTime.CreatedAt,
            tickets);
    }
}

/// <summary>
/// Validates query payload.
/// </summary>
public class GetShowTimeByIdValidator : AbstractValidator<GetShowTimeByIdQuery>
{
    public GetShowTimeByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ShowTime ID is required.");
    }
}
