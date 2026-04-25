using CinemaTicketBooking.Application.Common.Auth;

namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Loads a booking by id with ownership checks for the current user.
/// </summary>
public class GetBookingByIdQuery : IQuery<BookingDetailsDto?>
{
    public Guid BookingId { get; set; }
    public string CustomerSessionId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Returns booking summary when the caller is allowed to see it.
/// </summary>
public class GetBookingByIdHandler(IUnitOfWork uow, IUserContext userContext)
{
    /// <summary>
    /// Loads booking and enforces <see cref="PermissionHelper.EnsureCanAccessBooking"/>.
    /// </summary>
    public async Task<BookingDetailsDto?> Handle(GetBookingByIdQuery query, CancellationToken ct)
    {
        var booking = await uow.Bookings.LoadFullAsync(query.BookingId, ct);
        if (booking is null)
            return null;

        // If the booking's customer has the same session ID as the one provided, it means
        // the user who initiated the booking is checking its status, so we allow access.
        if (string.IsNullOrEmpty(query.CustomerSessionId) || booking.Customer?.SessionId != query.CustomerSessionId)
        {
            PermissionHelper.EnsureCanAccessBooking(userContext, booking);
        }

        return BuildViewResponse(booking);
    }

    private static BookingDetailsDto BuildViewResponse(Booking booking)
    {
        return new BookingDetailsDto
        {
            BookingId = booking.Id,
            ShowTimeId = booking.ShowTimeId,
            ShowTimeInfo = new ShowTimeInfo(
                booking.ShowTime!.Screen!.Code,
                booking.ShowTime!.Movie!.Name,
                booking.ShowTime!.StartAt,
                booking.ShowTime!.EndAt),
            OriginalAmount = booking.OriginAmount,
            DiscountAmount = booking.OriginAmount - booking.FinalAmount,
            FinalAmount = booking.FinalAmount,
            CheckinQrCode = booking.QrCode!,
            CreatedAt = booking.CreatedAt,
            Status = booking.Status,
            Tickets = booking.Tickets.Select(t => new TicketInfo(t.Ticket!.SeatCode, t.Ticket!.Price)).ToList(),
            TicketIds = booking.Tickets.Select(t => t.TicketId).ToList(),
            Concessions = booking.Concessions.Select(c => new ConcessionInfo(
                c.Concession!.Name, c.Concession!.ImageUrl, c.Concession!.Price, c.Quantity, c.Concession!.Price * c.Quantity))
                .ToList()
        };
    }
}

/// <summary>
/// Validates query input.
/// </summary>
public class GetBookingByIdQueryValidator : AbstractValidator<GetBookingByIdQuery>
{
    public GetBookingByIdQueryValidator()
    {
        RuleFor(x => x.BookingId).NotEmpty();
    }
}
