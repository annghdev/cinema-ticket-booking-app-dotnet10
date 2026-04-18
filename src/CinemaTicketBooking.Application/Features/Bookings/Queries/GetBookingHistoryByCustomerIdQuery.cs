using CinemaTicketBooking.Application.Common.Auth;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Application.Features;

public class GetBookingHistoryByCustomerIdQuery : IQuery<PagedResult<BookingMinimalInfoDto>>
{
    public Guid CustomerId { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public DateOnly? Date { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

public class GetBookingHistoryByCustomerIdHandler(IUnitOfWork uow, IUserContext userContext)
{
    public async Task<PagedResult<BookingMinimalInfoDto>> Handle(GetBookingHistoryByCustomerIdQuery query, CancellationToken ct)
    {
        PermissionHelper.EnsureCanAccessBooking(userContext, query.CustomerId);

        var bookingsQuery = uow.Bookings.GetQueryFilter()
            .Include(b=>b.ShowTime)
                .ThenInclude(st => st!.Screen)
            .Include(b => b.ShowTime)
                .ThenInclude(st => st!.Movie)
            .Where(b => b.CustomerId == query.CustomerId);
        if (query.Date.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.ShowTime!.Date == query.Date.Value);
        }
        var totalCount = await bookingsQuery.CountAsync(ct);
        var bookings = await bookingsQuery
            .OrderByDescending(b => b.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => new BookingMinimalInfoDto
            {
                BookingId = b.Id,
                ShowTimeInfo = new ShowTimeInfo(
                    b.ShowTime!.Screen!.Code,
                    b.ShowTime.Movie!.Name,
                    b.ShowTime.StartAt,
                    b.ShowTime.EndAt),
                FinalAmount = b.FinalAmount,
                CreatedAt = b.CreatedAt,
                Status = b.Status
            })
            .ToListAsync(ct);
        return new PagedResult<BookingMinimalInfoDto>(bookings, totalCount, query.PageNumber, query.PageSize);
    }
}