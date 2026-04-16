using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Application.Abstractions;

/// <summary>
/// Abstraction for payment gateway operations.
/// Each gateway implementation (VnPay, Momo, VietQR) implements this interface.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates a payment request and returns the payment link/QR data and redirect behavior.
    /// </summary>
    Task<CreatePaymentResult> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken ct = default);

    /// <summary>
    /// Validates and confirms payment result from gateway callback/return URL.
    /// </summary>
    Task<ConfirmPaymentResult> ConfirmPaymentAsync(ConfirmPaymentRequest request, CancellationToken ct = default);

    /// <summary>
    /// The payment method this service handles.
    /// </summary>
    PaymentMethod Method { get; }

    /// <summary>
    /// How the client should handle this gateway's payment flow.
    /// </summary>
    PaymentRedirectBehavior RedirectBehavior { get; }
}
