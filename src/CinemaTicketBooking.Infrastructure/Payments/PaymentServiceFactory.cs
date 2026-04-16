using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Infrastructure.Payments;

/// <summary>
/// Resolves payment services by method using a dictionary lookup.
/// </summary>
public sealed class PaymentServiceFactory(IEnumerable<IPaymentService> services) : IPaymentServiceFactory
{
    private readonly Dictionary<PaymentMethod, IPaymentService> _services =
        services.ToDictionary(s => s.Method);

    /// <summary>
    /// Resolves the payment service for the given method.
    /// </summary>
    public IPaymentService GetService(PaymentMethod method)
    {
        if (!_services.TryGetValue(method, out var service))
            throw new InvalidOperationException($"Payment method '{method}' is not registered.");
        return service;
    }

    /// <summary>
    /// Returns all registered payment gateway options for client display.
    /// </summary>
    public IReadOnlyList<PaymentGatewayOptionDto> GetAvailableOptions()
    {
        return _services.Values.Select(s => new PaymentGatewayOptionDto(
            Method: s.Method.ToString(),
            DisplayName: s.Method.ToString(),
            RedirectBehavior: s.RedirectBehavior)).ToList();
    }
}
