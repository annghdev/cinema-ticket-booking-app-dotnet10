using CinemaTicketBooking.Domain;
using CinemaTicketBooking.UnitTests.Shared;
using FluentAssertions;

namespace CinemaTicketBooking.UnitTests.EntityTests;

public class PricingPolicyTests
{
    [Fact]
    public void CalculatePrice_Should_ReturnBaseTimesCoefficient_When_ValuesAreSet()
    {
        var policy = DomainTestBuilders.Policy(SeatType.VIP, ScreenType.IMAX, basePrice: 100_000m, coefficient: 2.0m);
        policy.CalculatePrice().Should().Be(200_000m);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(10, 0)]
    [InlineData(10, -0.5)]
    public void UpdatePricing_Should_ThrowArgumentException_When_BasePriceOrCoefficientInvalid(
        decimal basePrice,
        decimal coefficient)
    {
        var policy = DomainTestBuilders.Policy(SeatType.Regular, ScreenType.TwoD);
        var act = () => policy.UpdatePricing(basePrice, coefficient);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdatePricing_Should_UpdateFieldsAndRaiseEvent_When_ValidNewPrices()
    {
        var policy = DomainTestBuilders.Policy(SeatType.Regular, ScreenType.TwoD, basePrice: 50_000m, coefficient: 1m);
        policy.UpdatePricing(75_000m, 1.5m);

        policy.BasePrice.Should().Be(75_000m);
        policy.ScreenCoefficient.Should().Be(1.5m);
        policy.Events.Should().ContainSingle().Which.Should().BeOfType<PricingPolicyUpdated>();
    }
}
