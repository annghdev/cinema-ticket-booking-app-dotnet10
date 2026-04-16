using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Infrastructure.Payments;

/// <summary>
/// Fake payment gateway for development. Always returns a success payment URL
/// with QrCode redirect behavior (simulates instant payment).
/// </summary>
public sealed class NoPaymentGatewayService : IPaymentService
{
    public PaymentMethod Method => PaymentMethod.None;
    public PaymentRedirectBehavior RedirectBehavior => PaymentRedirectBehavior.QrCode;

    /// <summary>
    /// Creates a fake payment with a generated QR code URL.
    /// </summary>
    public Task<CreatePaymentResult> CreatePaymentAsync(
        CreatePaymentRequest request, CancellationToken ct = default)
    {
        var fakeTransactionId = $"FAKE-{Guid.CreateVersion7()}";
        return Task.FromResult(new CreatePaymentResult(
            Success: true,
            PaymentUrl: $"https://fake-payment.local/pay/{fakeTransactionId}",
            GatewayTransactionId: fakeTransactionId,
            RedirectBehavior: PaymentRedirectBehavior.QrCode));
    }

    /// <summary>
    /// Always confirms payment as successful (fake flow).
    /// </summary>
    public Task<ConfirmPaymentResult> ConfirmPaymentAsync(
        ConfirmPaymentRequest request, CancellationToken ct = default)
    {
        return Task.FromResult(new ConfirmPaymentResult(
            IsSuccess: true,
            BookingId: request.BookingId,
            GatewayTransactionId: request.GatewayTransactionId));
    }
}
