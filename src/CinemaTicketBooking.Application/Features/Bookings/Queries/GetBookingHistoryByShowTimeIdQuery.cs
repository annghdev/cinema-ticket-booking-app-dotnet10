using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Query to fetch booking logs for an administrative view of a showtime.
/// </summary>
public class GetBookingHistoryByShowTimeIdQuery : IQuery<IReadOnlyList<ShowTimeBookingDto>>
{
    public Guid ShowTimeId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Details of individual bookings linked to a showtime.
/// </summary>
public class ShowTimeBookingDto
{
    public Guid BookingId { get; set; }
    public required string CustomerName { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal FinalAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public BookingStatus Status { get; set; }
    public List<string> SeatCodes { get; set; } = [];
}

/// <summary>
/// Processes retrieval of booking trails across individual showtime aggregates.
/// </summary>
public class GetBookingHistoryByShowTimeIdHandler(IUnitOfWork uow)
{
    public async Task<IReadOnlyList<ShowTimeBookingDto>> Handle(GetBookingHistoryByShowTimeIdQuery query, CancellationToken ct)
    {
        // 1. Gather distinct transactional entries
        var bookings = await uow.Bookings.GetQueryFilter()
            .Where(b => b.ShowTimeId == query.ShowTimeId)
            .Include(b => b.Tickets)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new ShowTimeBookingDto
            {
                BookingId = b.Id,
                CustomerName = b.CustomerName,
                PhoneNumber = b.PhoneNumber,
                Email = b.Email,
                FinalAmount = b.FinalAmount,
                CreatedAt = b.CreatedAt,
                Status = b.Status,
                SeatCodes = b.Tickets.Select(t => t.Ticket!.SeatCode).ToList()
            })
            .ToListAsync(ct);

        return bookings;
    }
}
