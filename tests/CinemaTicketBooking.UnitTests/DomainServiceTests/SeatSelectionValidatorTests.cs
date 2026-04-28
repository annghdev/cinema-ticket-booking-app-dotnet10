using CinemaTicketBooking.Domain;
using FluentAssertions;

namespace CinemaTicketBooking.UnitTests.DomainServiceTests;

public class SeatSelectionValidatorTests
{
    [Fact]
    public void Validate_Should_ReturnCanProceedTrue_When_SelectionIsValid()
    {
        var screen = BuildScreen("[[1,1,1,1,1]]");
        var showTime = BuildShowTime(screen, ("A1", TicketStatus.Locking, "session-1"), ("A2", TicketStatus.Locking, "session-1"));
        var policy = SeatSelectionPolicy.CreateDefault();
        var selectedIds = showTime.Tickets.Take(2).Select(x => x.Id).ToList();

        var result = SeatSelectionValidator.CreateDefault().Validate(showTime, policy, selectedIds, "session-1");

        result.CanProceed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_Should_Block_When_OrphanSeatIsCreated()
    {
        var screen = BuildScreen("[[1,1,1,1,1]]");
        var showTime = BuildShowTime(screen, ("A1", TicketStatus.Locking, "session-1"), ("A2", TicketStatus.Locking, "session-1"), ("A4", TicketStatus.Locking, "session-1"), ("A5", TicketStatus.Locking, "session-1"));
        var policy = SeatSelectionPolicy.CreateDefault();
        var selectedIds = showTime.Tickets.Select(x => x.Id).ToList();

        var result = SeatSelectionValidator.CreateDefault().Validate(showTime, policy, selectedIds, "session-1");

        result.CanProceed.Should().BeFalse();
        result.Errors.Should().Contain(x => x.Type == SeatSelectionViolationType.OrphanSeat);
    }

    [Fact]
    public void Validate_Should_Block_When_SelectionLeaves2SeatGap()
    {
        var screen = BuildScreen("[[1,1,1,1,1,1]]");
        var showTime = BuildShowTime(screen, ("A1", TicketStatus.Locking, "session-1"), ("A4", TicketStatus.Locking, "session-1"));
        var policy = SeatSelectionPolicy.CreateDefault();
        var selectedIds = showTime.Tickets.Where(x => x.SeatCode == "A1" || x.SeatCode == "A4").Select(x => x.Id).ToList();

        var result = SeatSelectionValidator.CreateDefault().Validate(showTime, policy, selectedIds, "session-1");

        result.CanProceed.Should().BeFalse();
        result.Errors.Should().Contain(x => x.Type == SeatSelectionViolationType.OrphanSeat);
    }

    [Fact]
    public void Validate_Should_ReturnWarning_When_IsolatedRowEndSingleDetected()
    {
        var screen = BuildScreen("[[1,1,1,1,1,1]]");
        var showTime = BuildShowTime(screen, ("A2", TicketStatus.Locking, "session-1"), ("A3", TicketStatus.Locking, "session-1"), ("A4", TicketStatus.Locking, "session-1"));
        var policy = SeatSelectionPolicy.CreateDefault();
        var selectedIds = showTime.Tickets.Select(x => x.Id).ToList();

        var result = SeatSelectionValidator.CreateDefault().Validate(showTime, policy, selectedIds, "session-1");

        result.CanProceed.Should().BeTrue();
        result.Warnings.Should().Contain(x => x.Type == SeatSelectionViolationType.IsolatedRowEndSingle);
    }

    [Fact]
    public void Validate_Should_Block_When_SelectedRowsExceedPolicy()
    {
        var screen = BuildScreen("[[1,1],[1,1],[1,1]]");
        var showTime = BuildShowTime(
            screen,
            ("A1", TicketStatus.Locking, "session-1"),
            ("B1", TicketStatus.Locking, "session-1"),
            ("C1", TicketStatus.Locking, "session-1"));
        var policy = SeatSelectionPolicy.CreateDefault();
        var selectedIds = showTime.Tickets.Select(x => x.Id).ToList();

        var result = SeatSelectionValidator.CreateDefault().Validate(showTime, policy, selectedIds, "session-1");

        result.CanProceed.Should().BeFalse();
        result.Errors.Should().Contain(x => x.Type == SeatSelectionViolationType.MaxRows);
    }

    private static Screen BuildScreen(string seatMap)
    {
        var screen = new Screen
        {
            Id = Guid.CreateVersion7(),
            CinemaId = Guid.CreateVersion7(),
            Code = "S1",
            Type = ScreenType.TwoD,
            IsActive = true,
            SeatMap = seatMap
        };
        screen.GenerateSeats(seatMap);
        return screen;
    }

    private static ShowTime BuildShowTime(
        Screen screen,
        params (string SeatCode, TicketStatus Status, string? LockingBy)[] selectedTicketStates)
    {
        var showTime = new ShowTime
        {
            Id = Guid.CreateVersion7(),
            ScreenId = screen.Id,
            Screen = screen,
            MovieId = Guid.CreateVersion7(),
            StartAt = DateTimeOffset.UtcNow.AddHours(2),
            EndAt = DateTimeOffset.UtcNow.AddHours(4),
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Status = ShowTimeStatus.Upcoming
        };

        foreach (var item in selectedTicketStates)
        {
            showTime.Tickets.Add(new Ticket
            {
                Id = Guid.CreateVersion7(),
                ShowTimeId = showTime.Id,
                SeatId = screen.Seats.FirstOrDefault(x => x.Code == item.SeatCode)?.Id,
                SeatCode = item.SeatCode,
                Status = item.Status,
                LockingBy = item.LockingBy,
                Code = $"20260415-S1-{item.SeatCode}",
                Description = $"{item.SeatCode} - Regular",
                Price = 100_000m
            });
        }

        return showTime;
    }
}
