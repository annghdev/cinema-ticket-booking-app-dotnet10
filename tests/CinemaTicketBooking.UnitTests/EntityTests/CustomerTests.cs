using CinemaTicketBooking.Domain;
using CinemaTicketBooking.UnitTests.Shared;
using FluentAssertions;

namespace CinemaTicketBooking.UnitTests.EntityTests;

public class CustomerTests
{
    [Fact]
    public void Register_Should_SetRegisteredAndRaiseEvent_When_CustomerIsGuest()
    {
        var customer = DomainTestBuilders.GuestCustomer();

        customer.Register();

        customer.IsRegistered.Should().BeTrue();
        customer.Events.Should().ContainSingle().Which.Should().BeOfType<CustomerRegistered>();
    }

    [Fact]
    public void Register_Should_Throw_When_CustomerIsAlreadyRegistered()
    {
        var customer = DomainTestBuilders.GuestCustomer();
        customer.Register();

        var act = () => customer.Register();
        act.Should().Throw<InvalidOperationException>();
    }
}
