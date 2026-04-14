using CinemaTicketBooking.Domain;
using FluentAssertions;

namespace CinemaTicketBooking.UnitTests.EntityTests;

public class MovieTests
{
    [Fact]
    public void PromoteToNowShowing_Should_SetNowShowingAndRaiseEvent_When_StatusIsOngoing()
    {
        var movie = new Movie
        {
            Id = Guid.CreateVersion7(),
            Name = "Inception",
            Status = MovieStatus.Upcoming
        };

        movie.PromoteToNowShowing();

        movie.Status.Should().Be(MovieStatus.NowShowing);
        movie.Events.Should().ContainSingle().Which.Should().BeOfType<MoviePromotedToNowShowing>();
    }

    [Fact]
    public void PromoteToNowShowing_Should_Throw_When_StatusIsNowShowing()
    {
        var movie = new Movie
        {
            Id = Guid.CreateVersion7(),
            Name = "M1",
            Status = MovieStatus.NowShowing
        };

        var act = () => movie.PromoteToNowShowing();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PromoteToNowShowing_Should_Throw_When_StatusIsNoShow()
    {
        var movie = new Movie
        {
            Id = Guid.CreateVersion7(),
            Name = "M2",
            Status = MovieStatus.NoShow
        };

        var act = () => movie.PromoteToNowShowing();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void WithdrawUpcomingRunAsNoShow_Should_SetNoShowAndRaiseEvent_When_StatusIsOngoing()
    {
        var movie = new Movie
        {
            Id = Guid.CreateVersion7(),
            Name = "Cancelled",
            Status = MovieStatus.Upcoming
        };

        movie.WithdrawUpcomingRunAsNoShow();

        movie.Status.Should().Be(MovieStatus.NoShow);
        movie.Events.Should().ContainSingle().Which.Should().BeOfType<MovieWithdrawnAsNoShowWhileUpcoming>();
    }

    [Fact]
    public void WithdrawUpcomingRunAsNoShow_Should_Throw_When_StatusIsNowShowing()
    {
        var movie = new Movie
        {
            Id = Guid.CreateVersion7(),
            Name = "M3",
            Status = MovieStatus.NowShowing
        };

        var act = () => movie.WithdrawUpcomingRunAsNoShow();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void WithdrawUpcomingRunAsNoShow_Should_Throw_When_StatusIsNoShow()
    {
        var movie = new Movie
        {
            Id = Guid.CreateVersion7(),
            Name = "M4",
            Status = MovieStatus.NoShow
        };

        var act = () => movie.WithdrawUpcomingRunAsNoShow();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CloseNowShowingRunAsNoShow_Should_SetNoShowAndRaiseEvent_When_StatusIsNowShowing()
    {
        var movie = new Movie
        {
            Id = Guid.CreateVersion7(),
            Name = "EndRun",
            Status = MovieStatus.NowShowing
        };

        movie.CloseNowShowingRunAsNoShow();

        movie.Status.Should().Be(MovieStatus.NoShow);
        movie.Events.Should().ContainSingle().Which.Should().BeOfType<MovieRunClosedAsNoShow>();
    }

    [Fact]
    public void CloseNowShowingRunAsNoShow_Should_Throw_When_StatusIsOngoing()
    {
        var movie = new Movie
        {
            Id = Guid.CreateVersion7(),
            Name = "M5",
            Status = MovieStatus.Upcoming
        };

        var act = () => movie.CloseNowShowingRunAsNoShow();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CloseNowShowingRunAsNoShow_Should_Throw_When_StatusIsNoShow()
    {
        var movie = new Movie
        {
            Id = Guid.CreateVersion7(),
            Name = "M6",
            Status = MovieStatus.NoShow
        };

        var act = () => movie.CloseNowShowingRunAsNoShow();
        act.Should().Throw<InvalidOperationException>();
    }
}
