using CinemaTicketBooking.Domain;
using CinemaTicketBooking.UnitTests.Shared;
using FluentAssertions;

namespace CinemaTicketBooking.UnitTests.EntityTests;

public class ShowTimeTests
{
    [Fact]
    public void Create_Should_ReturnShowTimeWithTickets_When_InvariantsHold()
    {
        var cinemaId = Guid.CreateVersion7();
        var movie = DomainTestBuilders.MovieNowShowing(100);
        var screen = DomainTestBuilders.ScreenWithSeatMap(cinemaId);
        var start = DomainTestBuilders.FutureStart();
        var policies = DomainTestBuilders.PoliciesForRegularAndVip(cinemaId, screen.Type);

        var showTime = ShowTime.Create(movie, screen, start, policies);

        showTime.MovieId.Should().Be(movie.Id);
        showTime.ScreenId.Should().Be(screen.Id);
        showTime.Status.Should().Be(ShowTimeStatus.Ongoing);
        showTime.Tickets.Should().HaveCount(screen.Seat.Count(s => s.IsActive));
        showTime.EndAt.Should().Be(start.Add(ShowTime.TrailerTime).Add(TimeSpan.FromMinutes(movie.Duration)));
        showTime.Events.Should().ContainSingle().Which.Should().BeOfType<ShowTimeCreated>();
    }

    [Fact]
    public void Create_Should_Throw_When_MovieIsNotNowShowing()
    {
        var cinemaId = Guid.CreateVersion7();
        var movie = DomainTestBuilders.MovieNowShowing();
        movie.Status = MovieStatus.Ongoing;
        var screen = DomainTestBuilders.ScreenWithSeatMap(cinemaId);
        var policies = DomainTestBuilders.PoliciesForRegularAndVip(cinemaId, screen.Type);

        var act = () => ShowTime.Create(movie, screen, DomainTestBuilders.FutureStart(), policies);
        act.Should().Throw<InvalidOperationException>().WithMessage("*not available for scheduling*");
    }

    [Fact]
    public void Create_Should_Throw_When_ScreenIsInactive()
    {
        var cinemaId = Guid.CreateVersion7();
        var movie = DomainTestBuilders.MovieNowShowing();
        var screen = DomainTestBuilders.ScreenWithSeatMap(cinemaId);
        screen.IsActive = false;
        var policies = DomainTestBuilders.PoliciesForRegularAndVip(cinemaId, screen.Type);

        var act = () => ShowTime.Create(movie, screen, DomainTestBuilders.FutureStart(), policies);
        act.Should().Throw<InvalidOperationException>().WithMessage("*not active*");
    }

    [Fact]
    public void Create_Should_Throw_When_StartTimeIsNotInFuture()
    {
        var cinemaId = Guid.CreateVersion7();
        var movie = DomainTestBuilders.MovieNowShowing();
        var screen = DomainTestBuilders.ScreenWithSeatMap(cinemaId);
        var policies = DomainTestBuilders.PoliciesForRegularAndVip(cinemaId, screen.Type);
        var past = DateTimeOffset.UtcNow.AddMinutes(-5);

        var act = () => ShowTime.Create(movie, screen, past, policies);
        act.Should().Throw<InvalidOperationException>().WithMessage("*future*");
    }

    [Fact]
    public void Create_Should_Throw_When_ScreenHasNoSeats()
    {
        var cinemaId = Guid.CreateVersion7();
        var movie = DomainTestBuilders.MovieNowShowing();
        var screen = new Screen
        {
            Id = Guid.CreateVersion7(),
            CinemaId = cinemaId,
            Code = "EMPTY",
            Type = ScreenType.TwoD,
            IsActive = true,
            Seat = []
        };
        var policies = DomainTestBuilders.PoliciesForRegularAndVip(cinemaId, screen.Type);

        var act = () => ShowTime.Create(movie, screen, DomainTestBuilders.FutureStart(), policies);
        act.Should().Throw<InvalidOperationException>().WithMessage("*no seats*");
    }

    [Fact]
    public void Create_Should_Throw_When_SeatTypeHasNoMatchingPricingPolicy()
    {
        var cinemaId = Guid.CreateVersion7();
        var movie = DomainTestBuilders.MovieNowShowing();
        var screen = DomainTestBuilders.ScreenWithSeatMap(cinemaId, seatMapJson: "[[2,0]]");
        var policies = new List<PricingPolicy>
        {
            DomainTestBuilders.Policy(SeatType.Regular, screen.Type, cinemaId)
        };

        var act = () => ShowTime.Create(movie, screen, DomainTestBuilders.FutureStart(), policies);
        act.Should().Throw<InvalidOperationException>().WithMessage("*No pricing policy found*");
    }

    [Fact]
    public void ConflictsWith_Should_ReturnFalse_When_ScreenIdsDiffer()
    {
        var a = new ShowTime
        {
            ScreenId = Guid.CreateVersion7(),
            StartAt = DateTimeOffset.Parse("2026-04-13T10:00:00Z"),
            EndAt = DateTimeOffset.Parse("2026-04-13T12:00:00Z"),
            Status = ShowTimeStatus.Ongoing
        };
        var b = new ShowTime
        {
            ScreenId = Guid.CreateVersion7(),
            StartAt = DateTimeOffset.Parse("2026-04-13T10:30:00Z"),
            EndAt = DateTimeOffset.Parse("2026-04-13T11:30:00Z"),
            Status = ShowTimeStatus.Ongoing
        };

        a.ConflictsWith(b).Should().BeFalse();
    }

    [Fact]
    public void ConflictsWith_Should_ReturnFalse_When_OtherShowTimeIsCancelled()
    {
        var screenId = Guid.CreateVersion7();
        var a = new ShowTime
        {
            ScreenId = screenId,
            StartAt = DateTimeOffset.Parse("2026-04-13T10:00:00Z"),
            EndAt = DateTimeOffset.Parse("2026-04-13T12:00:00Z"),
            Status = ShowTimeStatus.Ongoing
        };
        var b = new ShowTime
        {
            ScreenId = screenId,
            StartAt = DateTimeOffset.Parse("2026-04-13T10:30:00Z"),
            EndAt = DateTimeOffset.Parse("2026-04-13T11:30:00Z"),
            Status = ShowTimeStatus.Cancelled
        };

        a.ConflictsWith(b).Should().BeFalse();
    }

    [Fact]
    public void ConflictsWith_Should_ReturnTrue_When_IntervalsOverlapIncludingCleanupBuffer()
    {
        var screenId = Guid.CreateVersion7();
        var a = new ShowTime
        {
            ScreenId = screenId,
            StartAt = DateTimeOffset.Parse("2026-04-13T10:00:00Z"),
            EndAt = DateTimeOffset.Parse("2026-04-13T11:00:00Z"),
            Status = ShowTimeStatus.Ongoing
        };
        var b = new ShowTime
        {
            ScreenId = screenId,
            StartAt = DateTimeOffset.Parse("2026-04-13T11:10:00Z"),
            EndAt = DateTimeOffset.Parse("2026-04-13T12:00:00Z"),
            Status = ShowTimeStatus.Ongoing
        };

        a.ConflictsWith(b).Should().BeTrue();
    }

    [Fact]
    public void StartShowing_Should_SetShowingWithEvent_When_StatusIsOngoing()
    {
        var showTime = new ShowTime
        {
            Id = Guid.CreateVersion7(),
            MovieId = Guid.CreateVersion7(),
            ScreenId = Guid.CreateVersion7(),
            Date = new DateOnly(2026, 4, 13),
            StartAt = DateTimeOffset.Parse("2026-04-13T10:00:00Z"),
            EndAt = DateTimeOffset.Parse("2026-04-13T12:00:00Z"),
            Status = ShowTimeStatus.Ongoing
        };

        showTime.StartShowing();

        showTime.Status.Should().Be(ShowTimeStatus.Showing);
        showTime.Events.Should().Contain(e => e is ShowTimeStarted);
    }

    [Fact]
    public void Complete_Should_SetCompletedWithEvent_When_StatusIsShowing()
    {
        var showTime = new ShowTime
        {
            Id = Guid.CreateVersion7(),
            MovieId = Guid.CreateVersion7(),
            ScreenId = Guid.CreateVersion7(),
            Date = new DateOnly(2026, 4, 13),
            StartAt = DateTimeOffset.Parse("2026-04-13T10:00:00Z"),
            EndAt = DateTimeOffset.Parse("2026-04-13T12:00:00Z"),
            Status = ShowTimeStatus.Showing
        };

        showTime.Complete();

        showTime.Status.Should().Be(ShowTimeStatus.Completed);
        showTime.Events.Should().Contain(e => e is ShowTimeCompleted);
    }

    [Fact]
    public void Cancel_Should_Throw_When_StatusIsShowing()
    {
        var showing = new ShowTime
        {
            Status = ShowTimeStatus.Showing,
            Movie = DomainTestBuilders.MovieNowShowing(),
            Screen = DomainTestBuilders.ScreenWithSeatMap(Guid.CreateVersion7())
        };

        var act = () => showing.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_Should_Throw_When_StatusIsCompleted()
    {
        var completed = new ShowTime { Status = ShowTimeStatus.Completed };

        var act = () => completed.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_Should_SetCancelledWithEvent_When_StatusIsOngoing()
    {
        var movie = DomainTestBuilders.MovieNowShowing();
        var screen = DomainTestBuilders.ScreenWithSeatMap(Guid.CreateVersion7());
        var showTime = new ShowTime
        {
            Id = Guid.CreateVersion7(),
            MovieId = movie.Id,
            Movie = movie,
            ScreenId = screen.Id,
            Screen = screen,
            Date = new DateOnly(2026, 4, 13),
            StartAt = DateTimeOffset.Parse("2026-04-13T10:00:00Z"),
            Status = ShowTimeStatus.Ongoing
        };

        showTime.Cancel();

        showTime.Status.Should().Be(ShowTimeStatus.Cancelled);
        showTime.Events.Should().Contain(e => e is ShowTimeCancelled);
    }
}
