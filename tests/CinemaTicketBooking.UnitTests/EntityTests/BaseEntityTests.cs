using CinemaTicketBooking.Domain;
using CinemaTicketBooking.UnitTests.Shared;
using FluentAssertions;

namespace CinemaTicketBooking.UnitTests.EntityTests;

/// <summary>
/// Tests for BaseEntity domain event collection behavior (via AuditableEntity).
/// </summary>
public class BaseEntityTests
{
    [Fact]
    public void ClearEvents_Should_RemoveAllEvents_When_CalledAfterDomainEventRaised()
    {
        var policy = DomainTestBuilders.Policy(SeatType.Regular, ScreenType.TwoD);
        policy.UpdatePricing(60_000m, 1.2m);

        policy.Events.Should().ContainSingle()
            .Which.Should().BeOfType<PricingPolicyUpdated>();

        policy.ClearEvents();
        policy.Events.Should().BeEmpty();
    }

    [Fact]
    public void UpdatePricing_Should_AccumulateEvents_When_CalledMultipleTimes_Until_ClearEvents()
    {
        var policy = DomainTestBuilders.Policy(SeatType.Regular, ScreenType.TwoD);
        policy.UpdatePricing(10m, 1m);
        policy.UpdatePricing(20m, 2m);

        policy.Events.Should().HaveCount(2);
    }
}
