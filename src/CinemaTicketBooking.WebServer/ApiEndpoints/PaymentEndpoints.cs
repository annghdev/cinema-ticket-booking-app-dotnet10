using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Application.Features.Bookings.Commands;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CinemaTicketBooking.WebServer.ApiEndpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments");

        group.MapGet("/momo-callback", MomoCallback)
            .AllowAnonymous();

        group.MapGet("/fake-callback", FakeCallback)
            .AllowAnonymous();

        group.MapGet("/gateways", GetAvailableGateways)
            .AllowAnonymous();
    }

    public static async Task<IResult> FakeCallback(
        HttpRequest request,
        IMessageBus bus,
        [FromQuery] Guid bookingId,
        [FromQuery] string transactionId)
    {
        var gatewayParams = request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());

        var command = new VerifyPaymentCommand
        {
            BookingId = bookingId,
            GatewayTransactionId = transactionId,
            PaymentMethod = "None",
            GatewayResponseParams = gatewayParams
        };

        var response = await bus.InvokeAsync<VerifyPaymentResponse>(command);

        if (response.IsSuccess)
        {
            return Results.Ok(response);
        }

        return Results.BadRequest(response);
    }

    public static async Task<IResult> MomoCallback()
    {
        return Results.Ok();
    }

    public static async Task<IResult> GetAvailableGateways(IMessageBus bus)
    {
        var gateways = await bus.InvokeAsync<IReadOnlyList<PaymentGatewayOptionDto>>(new GetAvailableGatewaysQuery());
        return Results.Ok(gateways);
    }
}
