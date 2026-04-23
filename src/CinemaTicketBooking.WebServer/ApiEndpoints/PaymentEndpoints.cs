using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Application.Features.Bookings.Commands;
using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Domain;
using CinemaTicketBooking.Infrastructure.Payments.Vnpay;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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

        group.MapGet("/vnpay-ipn", VnpayIpn)
            .AllowAnonymous();

        group.MapGet("/vnpay-return", VnpayReturn)
            .AllowAnonymous();

        group.MapGet("/result", GetPaymentResult)
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

    public static async Task<IResult> VnpayIpn(
        HttpRequest request,
        IMessageBus bus,
        IUnitOfWork uow)
    {
        var gatewayParams = request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());

        if (!gatewayParams.TryGetValue("vnp_TxnRef", out var txnRef) || string.IsNullOrWhiteSpace(txnRef))
        {
            return Results.Ok(new VnpayIpnResponse("99", "Invalid request"));
        }

        if (!gatewayParams.TryGetValue("vnp_Amount", out var amountRaw)
            || !long.TryParse(amountRaw, out var gatewayAmount))
        {
            return Results.Ok(new VnpayIpnResponse("99", "Invalid amount"));
        }

        var transaction = await uow.PaymentTransactions.GetByGatewayTransactionIdAsync(txnRef, default);
        if (transaction is null)
        {
            return Results.Ok(new VnpayIpnResponse("01", "Order not found"));
        }

        var expectedAmount = Convert.ToInt64(decimal.Round(transaction.Amount * 100m, 0, MidpointRounding.AwayFromZero));
        if (expectedAmount != gatewayAmount)
        {
            return Results.Ok(new VnpayIpnResponse("04", "Invalid amount"));
        }

        if (transaction.Status != PaymentTransactionStatus.Pending)
        {
            return Results.Ok(new VnpayIpnResponse("02", "Order already confirmed"));
        }

        var command = new VerifyPaymentCommand
        {
            BookingId = transaction.BookingId,
            GatewayTransactionId = txnRef,
            PaymentMethod = PaymentMethod.VnPay.ToString(),
            GatewayResponseParams = gatewayParams
        };

        try
        {
            var response = await bus.InvokeAsync<VerifyPaymentResponse>(command);
            if (response.IsSuccess)
            {
                return Results.Ok(new VnpayIpnResponse("00", "Confirm Success"));
            }

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage)
                && response.ErrorMessage.Contains("signature", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Ok(new VnpayIpnResponse("97", "Invalid signature"));
            }

            // Merchant processed payment result (including fail status), so return success to stop retries.
            return Results.Ok(new VnpayIpnResponse("00", "Confirm Success"));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("signature", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Ok(new VnpayIpnResponse("97", "Invalid signature"));
        }
        catch
        {
            return Results.Ok(new VnpayIpnResponse("99", "Unknown error"));
        }
    }

    public static async Task<IResult> VnpayReturn(
        HttpRequest request,
        IMessageBus bus,
        IUnitOfWork uow,
        IOptions<VnpayOptions> options)
    {
        var gatewayParams = request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
        var txnRef = gatewayParams.TryGetValue("vnp_TxnRef", out var value) ? value : string.Empty;
        var transaction = string.IsNullOrWhiteSpace(txnRef)
            ? null
            : await uow.PaymentTransactions.GetByGatewayTransactionIdAsync(txnRef, default);

        VerifyPaymentResponse? verifyResponse = null;
        if (transaction is not null && transaction.Status == PaymentTransactionStatus.Pending)
        {
            try
            {
                verifyResponse = await bus.InvokeAsync<VerifyPaymentResponse>(new VerifyPaymentCommand
                {
                    BookingId = transaction.BookingId,
                    GatewayTransactionId = txnRef,
                    PaymentMethod = PaymentMethod.VnPay.ToString(),
                    GatewayResponseParams = gatewayParams
                });
            }
            catch
            {
                // Ignore verification failure here; frontend will query backend result endpoint.
            }
        }

        var responseCode = gatewayParams.TryGetValue("vnp_ResponseCode", out var code) ? code : string.Empty;
        var transactionStatus = gatewayParams.TryGetValue("vnp_TransactionStatus", out var statusCode) ? statusCode : string.Empty;
        var isSuccess = verifyResponse?.IsSuccess
                        ?? (responseCode == "00" && (string.IsNullOrWhiteSpace(transactionStatus) || transactionStatus == "00"));

        var frontendUrl = string.IsNullOrWhiteSpace(options.Value.FrontendResultUrl)
            ? "http://localhost:3000/payment-result"
            : options.Value.FrontendResultUrl;
        var redirectUrl = QueryHelpers.AddQueryString(frontendUrl, new Dictionary<string, string?>
        {
            ["status"] = isSuccess ? "success" : "failed",
            ["bookingId"] = transaction?.BookingId.ToString(),
            ["txnRef"] = txnRef,
            ["responseCode"] = responseCode
        });

        return Results.Redirect(redirectUrl);
    }

    public static async Task<IResult> GetPaymentResult(
        [FromQuery] Guid? bookingId,
        [FromQuery] string txnRef,
        IUnitOfWork uow)
    {
        if (string.IsNullOrWhiteSpace(txnRef))
        {
            return Results.BadRequest(new PaymentResultLookupResponse(
                BookingId: null,
                IsSuccess: false,
                Status: "invalid_request",
                CheckinQrCode: null,
                Booking: null,
                ErrorMessage: "txnRef is required.",
                CanRetry: false));
        }

        var transaction = await uow.PaymentTransactions.GetByGatewayTransactionIdAsync(txnRef, default);
        if (transaction is null)
        {
            return Results.NotFound(new PaymentResultLookupResponse(
                BookingId: bookingId,
                IsSuccess: false,
                Status: "not_found",
                CheckinQrCode: null,
                Booking: null,
                ErrorMessage: "Payment transaction was not found.",
                CanRetry: false));
        }

        if (bookingId.HasValue && bookingId.Value != Guid.Empty && transaction.BookingId != bookingId.Value)
        {
            return Results.NotFound(new PaymentResultLookupResponse(
                BookingId: bookingId,
                IsSuccess: false,
                Status: "not_found",
                CheckinQrCode: null,
                Booking: null,
                ErrorMessage: "Payment transaction does not belong to booking.",
                CanRetry: false));
        }

        var resolvedBookingId = transaction.BookingId;
        var booking = await uow.Bookings.LoadFullAsync(resolvedBookingId, default);
        if (booking is null)
        {
            return Results.NotFound(new PaymentResultLookupResponse(
                BookingId: resolvedBookingId,
                IsSuccess: false,
                Status: "not_found",
                CheckinQrCode: null,
                Booking: null,
                ErrorMessage: "Booking was not found.",
                CanRetry: false));
        }

        var isSuccess = transaction.Status == PaymentTransactionStatus.Success
                        && booking.Status == BookingStatus.Confirmed
                        && !string.IsNullOrWhiteSpace(booking.QrCode);

        var responseStatus = isSuccess
            ? "confirmed"
            : transaction.Status == PaymentTransactionStatus.Pending
                ? "pending_payment"
                : "payment_failed";

        return Results.Ok(new PaymentResultLookupResponse(
            BookingId: booking.Id,
            IsSuccess: isSuccess,
            Status: responseStatus,
            CheckinQrCode: booking.QrCode,
            Booking: BuildBookingDetailsResponse(booking),
            ErrorMessage: isSuccess ? null : "Payment has not been confirmed yet.",
            CanRetry: !isSuccess));
    }

    public static async Task<IResult> GetAvailableGateways(IMessageBus bus)
    {
        var gateways = await bus.InvokeAsync<IReadOnlyList<PaymentGatewayOptionDto>>(new GetAvailableGatewaysQuery());
        return Results.Ok(gateways);
    }

    private static BookingDetailsDto BuildBookingDetailsResponse(Booking booking)
    {
        return new BookingDetailsDto
        {
            BookingId = booking.Id,
            ShowTimeInfo = new ShowTimeInfo(
                booking.ShowTime!.Screen!.Code,
                booking.ShowTime.Movie!.Name,
                booking.ShowTime.StartAt,
                booking.ShowTime.EndAt),
            OriginalAmount = booking.OriginAmount,
            DiscountAmount = booking.OriginAmount - booking.FinalAmount,
            FinalAmount = booking.FinalAmount,
            CheckinQrCode = booking.QrCode ?? string.Empty,
            CreatedAt = booking.CreatedAt,
            Status = booking.Status,
            Tickets = booking.Tickets.Select(t => new TicketInfo(t.Ticket!.SeatCode, t.Ticket.Price)).ToList(),
            Concessions = booking.Concessions.Select(c => new ConcessionInfo(
                c.Concession!.Name,
                c.Concession.ImageUrl,
                c.Concession.Price,
                c.Quantity,
                c.Concession.Price * c.Quantity)).ToList()
        };
    }
}

public sealed record VnpayIpnResponse(string RspCode, string Message);
public sealed record PaymentResultLookupResponse(
    Guid? BookingId,
    bool IsSuccess,
    string Status,
    string? CheckinQrCode,
    BookingDetailsDto? Booking,
    string? ErrorMessage,
    bool CanRetry);
