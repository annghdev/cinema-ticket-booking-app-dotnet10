using CinemaTicketBooking.Domain;
using FluentAssertions;

namespace CinemaTicketBooking.UnitTests.EntityTests;

public class ConcessionTests
{
    [Fact]
    public void MarkAsUnavailable_Should_SetUnavailableAndRaiseEvent_When_Available()
    {
        var concession = new Concession
        {
            Id = Guid.CreateVersion7(),
            Name = "Popcorn",
            Price = 25_000m,
            IsAvailable = true
        };

        concession.MarkAsUnavailable();

        concession.IsAvailable.Should().BeFalse();
        concession.Events.Should().ContainSingle().Which.Should().BeOfType<ConcessionMarkedUnavailable>();
    }

    [Fact]
    public void MarkAsAvailable_Should_SetAvailableAndRaiseEvent_When_Unavailable()
    {
        var concession = new Concession
        {
            Id = Guid.CreateVersion7(),
            Name = "Soda",
            Price = 15_000m,
            IsAvailable = false
        };

        concession.MarkAsAvailable();

        concession.IsAvailable.Should().BeTrue();
        concession.Events.Should().ContainSingle().Which.Should().BeOfType<ConcessionMarkedAvailable>();
    }

    [Fact]
    public void MarkAsUnavailable_Should_BeNoOpWithoutEvent_When_AlreadyUnavailable()
    {
        var concession = new Concession
        {
            Id = Guid.CreateVersion7(),
            Name = "Candy",
            Price = 5_000m,
            IsAvailable = false
        };

        concession.MarkAsUnavailable();

        concession.IsAvailable.Should().BeFalse();
        concession.Events.Should().BeEmpty();
    }

    [Fact]
    public void MarkAsAvailable_Should_BeNoOpWithoutEvent_When_AlreadyAvailable()
    {
        var concession = new Concession
        {
            Id = Guid.CreateVersion7(),
            Name = "Nachos",
            Price = 40_000m,
            IsAvailable = true
        };

        concession.MarkAsAvailable();

        concession.IsAvailable.Should().BeTrue();
        concession.Events.Should().BeEmpty();
    }
}
