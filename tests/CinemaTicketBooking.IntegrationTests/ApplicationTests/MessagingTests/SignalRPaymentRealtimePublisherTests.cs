using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.WebServer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace CinemaTicketBooking.IntegrationTests.ApplicationTests.MessagingTests;

public sealed class SignalRPaymentRealtimePublisherTests
{
    [Fact]
    public async Task PublishPaymentConfirmedAsync_Should_SendToBookingGroup()
    {
        var bookingId = Guid.CreateVersion7();
        var clientProxy = new Mock<IClientProxy>();
        var clients = new Mock<IHubClients>();
        clients.Setup(x => x.Group(PaymentHub.BuildBookingGroup(bookingId))).Returns(clientProxy.Object);

        var hubContext = new Mock<IHubContext<PaymentHub>>();
        hubContext.SetupGet(x => x.Clients).Returns(clients.Object);
        var publisher = new SignalRPaymentRealtimePublisher(hubContext.Object);

        var @event = new PaymentConfirmedRealtimeEvent(
            bookingId,
            Guid.CreateVersion7(),
            "GW-TXN-01",
            "confirmed",
            "qr://booking",
            DateTimeOffset.UtcNow);

        await publisher.PublishPaymentConfirmedAsync(@event, CancellationToken.None);

        clients.Verify(x => x.Group(PaymentHub.BuildBookingGroup(bookingId)), Times.Once);
        clientProxy.Verify(
            x => x.SendCoreAsync(
                PaymentHub.PaymentConfirmedEvent,
                It.Is<object[]>(args => args.Length == 1 && Equals(args[0], @event)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
