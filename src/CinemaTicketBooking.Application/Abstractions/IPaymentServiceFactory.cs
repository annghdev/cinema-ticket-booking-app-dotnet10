using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Application.Abstractions;

/// <summary>
/// Factory for resolving payment service by method.
/// </summary>
public interface IPaymentServiceFactory
{
    /// <summary>
    /// Resolves the payment service for the given method.
    /// Throws if the method is not registered.
    /// </summary>
    IPaymentService GetService(PaymentMethod method);

    /// <summary>
    /// Returns all registered payment gateway options for client display.
    /// </summary>
    IReadOnlyList<PaymentGatewayOptionDto> GetAvailableOptions();
}
