namespace CinemaTicketBooking.Application.Features;

public class GetAvailableGatewaysQuery
     : IQuery<IReadOnlyList<PaymentGatewayOptionDto>>
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}


public class GetAvailableGatewaysHandler(IPaymentServiceFactory gatewayFactory)
{
    public IReadOnlyList<PaymentGatewayOptionDto> Handle(GetAvailableGatewaysQuery query, CancellationToken ct)
    {
        return gatewayFactory.GetAvailableOptions();
    }
}