using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.IntegrationTests.Shared.Fixtures;

public sealed class MoviePromotedToNowShowingHandler(TestDomainEventHandlerProbe probe)
{
    public Task Handle(MoviePromotedToNowShowing domainEvent)
    {
        probe.MarkHandled(domainEvent);
        return Task.CompletedTask;
    }
}
