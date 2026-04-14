using CinemaTicketBooking.Domain;
using CinemaTicketBooking.UnitTests.Shared;
using FluentAssertions;
using Moq;

namespace CinemaTicketBooking.UnitTests.DomainServiceTests;

public class ShowTimeSchedulingServiceTests
{
    [Fact]
    public async Task ScheduleAsync_Should_Throw_When_NoActivePricingPolicies()
    {
        var cinemaId = Guid.CreateVersion7();
        var movie = DomainTestBuilders.MovieNowShowing();
        var screen = DomainTestBuilders.ScreenWithSeatMap(cinemaId);
        var start = DomainTestBuilders.FutureStart();

        var showTimeRepo = new Mock<IShowTimeRepository>();
        var pricingRepo = new Mock<IPricingPolicyRepository>();
        pricingRepo
            .Setup(r => r.GetActivePoliciesAsync(cinemaId, screen.Type, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var sut = new ShowTimeSchedulingService(showTimeRepo.Object, pricingRepo.Object);

        var act = async () => await sut.ScheduleAsync(movie, screen, start);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No pricing policies found*");
        showTimeRepo.Verify(
            r => r.GetActiveByScreenAndDateRangeAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ScheduleAsync_Should_Throw_When_ConflictingShowTimeExists()
    {
        var cinemaId = Guid.CreateVersion7();
        var movie = DomainTestBuilders.MovieNowShowing(60);
        var screen = DomainTestBuilders.ScreenWithSeatMap(cinemaId);
        var start = DomainTestBuilders.FutureStart(4);
        var policies = DomainTestBuilders.PoliciesForRegularAndVip(cinemaId, screen.Type);

        var conflicting = new ShowTime
        {
            Id = Guid.CreateVersion7(),
            ScreenId = screen.Id,
            StartAt = start.AddHours(-1),
            EndAt = start.AddHours(3),
            Status = ShowTimeStatus.Upcoming
        };

        var showTimeRepo = new Mock<IShowTimeRepository>();
        showTimeRepo
            .Setup(r => r.GetActiveByScreenAndDateRangeAsync(
                screen.Id,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([conflicting]);

        var pricingRepo = new Mock<IPricingPolicyRepository>();
        pricingRepo
            .Setup(r => r.GetActivePoliciesAsync(cinemaId, screen.Type, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policies);

        var sut = new ShowTimeSchedulingService(showTimeRepo.Object, pricingRepo.Object);

        var act = async () => await sut.ScheduleAsync(movie, screen, start);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Schedule conflict*");
    }

    [Fact]
    public async Task ScheduleAsync_Should_ReturnShowTime_When_NoConflicts()
    {
        var cinemaId = Guid.CreateVersion7();
        var movie = DomainTestBuilders.MovieNowShowing();
        var screen = DomainTestBuilders.ScreenWithSeatMap(cinemaId);
        var start = DomainTestBuilders.FutureStart();
        var policies = DomainTestBuilders.PoliciesForRegularAndVip(cinemaId, screen.Type);

        var showTimeRepo = new Mock<IShowTimeRepository>();
        showTimeRepo
            .Setup(r => r.GetActiveByScreenAndDateRangeAsync(
                screen.Id,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var pricingRepo = new Mock<IPricingPolicyRepository>();
        pricingRepo
            .Setup(r => r.GetActivePoliciesAsync(cinemaId, screen.Type, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policies);

        var sut = new ShowTimeSchedulingService(showTimeRepo.Object, pricingRepo.Object);

        var result = await sut.ScheduleAsync(movie, screen, start);

        result.Should().NotBeNull();
        result.ScreenId.Should().Be(screen.Id);
        result.Tickets.Should().NotBeEmpty();
        showTimeRepo.Verify(
            r => r.GetActiveByScreenAndDateRangeAsync(
                screen.Id,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
