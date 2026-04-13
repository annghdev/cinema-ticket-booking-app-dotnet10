using CinemaTicketBooking.Domain;
using FluentAssertions;

namespace CinemaTicketBooking.UnitTests.EntityTests;

public class CinemaTests
{
    [Fact]
    public void Deactivate_Should_SetInactiveAndRaiseEvent_When_Active()
    {
        var cinema = new Cinema
        {
            Id = Guid.CreateVersion7(),
            Name = "Cineplex",
            Address = "District1",
            IsActive = true
        };

        cinema.Deactivate();

        cinema.IsActive.Should().BeFalse();
        cinema.Events.Should().ContainSingle().Which.Should().BeOfType<CinemaDeactivated>();
    }

    [Fact]
    public void Activate_Should_SetActiveAndRaiseEvent_When_Inactive()
    {
        var cinema = new Cinema
        {
            Id = Guid.CreateVersion7(),
            Name = "Cineplex",
            Address = "District1",
            IsActive = false
        };

        cinema.Activate();

        cinema.IsActive.Should().BeTrue();
        cinema.Events.Should().ContainSingle().Which.Should().BeOfType<CinemaActivated>();
    }

    [Fact]
    public void Deactivate_Should_BeNoOpWithoutEvent_When_AlreadyInactive()
    {
        var cinema = new Cinema
        {
            Id = Guid.CreateVersion7(),
            Name = "C1",
            Address = "A",
            IsActive = false
        };

        cinema.Deactivate();

        cinema.IsActive.Should().BeFalse();
        cinema.Events.Should().BeEmpty();
    }

    [Fact]
    public void Activate_Should_BeNoOpWithoutEvent_When_AlreadyActive()
    {
        var cinema = new Cinema
        {
            Id = Guid.CreateVersion7(),
            Name = "C2",
            Address = "B",
            IsActive = true
        };

        cinema.Activate();

        cinema.IsActive.Should().BeTrue();
        cinema.Events.Should().BeEmpty();
    }
}
