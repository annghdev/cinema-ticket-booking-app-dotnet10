namespace CinemaTicketBooking.WebServer.ApiEndpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments");

        group.MapGet("/momo-callback", MomoCallback)
            .AllowAnonymous();
    }

    public static async Task<IResult> MomoCallback()
    {
        return Results.Ok();
    }
}
