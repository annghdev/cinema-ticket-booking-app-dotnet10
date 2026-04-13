using CinemaTicketBooking.Domain;
using FluentAssertions;

namespace CinemaTicketBooking.UnitTests.EntityTests;

public class TicketTests
{
    [Fact]
    public void Lock_Should_SetLockingStateAndRaiseEvent_When_TicketIsAvailable()
    {
        var ticket = new Ticket
        {
            Id = Guid.CreateVersion7(),
            ShowTimeId = Guid.CreateVersion7(),
            Code = "20260413-S1-A1",
            Price = 50_000m,
            Status = TicketStatus.Available
        };

        ticket.Lock("session-x");

        ticket.Status.Should().Be(TicketStatus.Locking);
        ticket.LockingBy.Should().Be("session-x");
        ticket.Events.Should().ContainSingle().Which.Should().BeOfType<TicketLocked>();
    }

    [Fact]
    public void Lock_Should_Throw_When_StatusIsNotAvailable()
    {
        var ticket = new Ticket
        {
            Id = Guid.CreateVersion7(),
            ShowTimeId = Guid.CreateVersion7(),
            Status = TicketStatus.Locking,
            LockingBy = "a"
        };

        var act = () => ticket.Lock("b");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Release_Should_ResetToAvailable_When_CalledWithoutOwnerOnLockingTicket()
    {
        var ticket = new Ticket
        {
            Id = Guid.CreateVersion7(),
            ShowTimeId = Guid.CreateVersion7(),
            Status = TicketStatus.Locking,
            LockingBy = "session-x"
        };

        ticket.Release();

        ticket.Status.Should().Be(TicketStatus.Available);
        ticket.LockingBy.Should().BeNull();
    }

    [Fact]
    public void Release_WithCaller_Should_Throw_When_TicketIsAvailable()
    {
        var ticket = new Ticket
        {
            Id = Guid.CreateVersion7(),
            ShowTimeId = Guid.CreateVersion7(),
            Status = TicketStatus.Available
        };

        var act = () => ticket.Release("session-x");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Release_WithCaller_Should_Throw_When_CallerDoesNotOwnLock()
    {
        var ticket = new Ticket
        {
            Id = Guid.CreateVersion7(),
            ShowTimeId = Guid.CreateVersion7(),
            Status = TicketStatus.Locking,
            LockingBy = "session-a"
        };

        var act = () => ticket.Release("session-b");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsSold_Should_SetSoldLinkBookingAndRaiseEvent_When_TicketIsLocking()
    {
        var ticket = new Ticket
        {
            Id = Guid.CreateVersion7(),
            ShowTimeId = Guid.CreateVersion7(),
            Status = TicketStatus.Locking,
            LockingBy = "x"
        };
        var bookingId = Guid.CreateVersion7();

        ticket.MarkAsSold(bookingId);

        ticket.Status.Should().Be(TicketStatus.Sold);
        ticket.BookingId.Should().Be(bookingId);
        ticket.LockingBy.Should().BeNull();
        ticket.Events.Should().ContainSingle().Which.Should().BeOfType<TicketSold>();
    }

    [Fact]
    public void MarkAsSold_Should_Throw_When_StatusIsNotLocking()
    {
        var ticket = new Ticket
        {
            Id = Guid.CreateVersion7(),
            ShowTimeId = Guid.CreateVersion7(),
            Status = TicketStatus.Available
        };

        var act = () => ticket.MarkAsSold(Guid.CreateVersion7());
        act.Should().Throw<InvalidOperationException>();
    }
}
