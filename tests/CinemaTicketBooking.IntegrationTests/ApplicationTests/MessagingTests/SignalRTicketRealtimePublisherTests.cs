using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Domain;
using CinemaTicketBooking.WebServer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace CinemaTicketBooking.IntegrationTests.ApplicationTests.MessagingTests;

public sealed class SignalRTicketRealtimePublisherTests
{
    [Fact]
    public async Task PublishTicketStatusChangedAsync_Should_SendToMatchingShowtimeGroup()
    {
        var showTimeId = Guid.CreateVersion7();
        var clientProxy = new Mock<IClientProxy>();
        var clients = new Mock<IHubClients>();
        clients.Setup(x => x.Group(TicketStatusHub.BuildShowTimeGroup(showTimeId))).Returns(clientProxy.Object);

        var hubContext = new Mock<IHubContext<TicketStatusHub>>();
        hubContext.SetupGet(x => x.Clients).Returns(clients.Object);
        var publisher = new SignalRTicketRealtimePublisher(hubContext.Object);

        var @event = new TicketStatusChangedRealtimeEvent(
            showTimeId,
            Guid.CreateVersion7(),
            "A1",
            TicketStatus.Locking,
            DateTimeOffset.UtcNow);

        await publisher.PublishTicketStatusChangedAsync(@event, CancellationToken.None);

        clients.Verify(x => x.Group(TicketStatusHub.BuildShowTimeGroup(showTimeId)), Times.Once);
        clientProxy.Verify(
            x => x.SendCoreAsync(
                TicketStatusHub.TicketStatusChangedEvent,
                It.Is<object[]>(args => args.Length == 1 && Equals(args[0], @event)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void BuildShowTimeGroup_Should_UseDistinctGroupPerShowtime()
    {
        var groupA = TicketStatusHub.BuildShowTimeGroup(Guid.Parse("019d9bfc-8906-7623-8706-b3216a756417"));
        var groupB = TicketStatusHub.BuildShowTimeGroup(Guid.Parse("019d9bfc-8906-7623-8706-b3216a756418"));

        groupA.Should().NotBe(groupB);
        groupA.Should().StartWith("showtime:");
        groupB.Should().StartWith("showtime:");
    }
}
