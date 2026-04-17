using CinemaTicketBooking.Application.Features.Bookings.Queries;
using Wolverine;

namespace CinemaTicketBooking.WebServer.ApiEndpoints;

/// <summary>
/// Customer booking read APIs.
/// </summary>
public static class BookingEndpoints
{
    /// <summary>
    /// Maps booking routes.
    /// </summary>
    public static void MapBookingEndpoints(this WebApplication app)
    {
        app.MapGet(
                "/api/bookings/{bookingId:guid}",
                async (Guid bookingId, IMessageBus bus, CancellationToken ct) =>
                {
                    try
                    {
                        var dto = await bus.InvokeAsync<BookingSummaryDto?>(
                            new GetBookingByIdQuery
                            {
                                BookingId = bookingId,
                                CorrelationId = string.Empty
                            },
                            ct);
                        return dto is null ? Results.NotFound() : Results.Ok(dto);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return Results.Forbid();
                    }
                })
            .RequireAuthorization()
            .WithTags("Bookings");
    }
}
