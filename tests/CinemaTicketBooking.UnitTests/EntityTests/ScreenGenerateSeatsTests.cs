using CinemaTicketBooking.Domain;
using FluentAssertions;

namespace CinemaTicketBooking.UnitTests.EntityTests;

public class ScreenGenerateSeatsTests
{
    [Fact]
    public void GenerateSeats_Should_CreateSeatsRightToLeft_When_JsonMapContainsStairColumn()
    {
        var cinemaId = Guid.CreateVersion7();
        var screen = new Screen
        {
            Id = Guid.CreateVersion7(),
            CinemaId = cinemaId,
            Code = "S1",
            Type = ScreenType.TwoD,
            IsActive = true
        };

        screen.GenerateSeats("[[1,1,0]]");

        screen.Seat.Should().HaveCount(2);
        screen.Seat.Select(s => s.Code).Should().Contain("A1", "A2");
        screen.Seat.Should().OnlyContain(s => s.Type == SeatType.Regular);
        screen.Events.Should().ContainSingle().Which.Should().BeOfType<ScreenSeatsGenerated>();
    }

    [Fact]
    public void GenerateSeats_Should_ParsePlainTextRows_When_NewlineSeparatedValues()
    {
        var screen = new Screen
        {
            Id = Guid.CreateVersion7(),
            CinemaId = Guid.CreateVersion7(),
            Code = "S2",
            Type = ScreenType.TwoD,
            IsActive = true
        };

        var plain = "1 1 0\n1 1 0";
        screen.GenerateSeats(plain);

        screen.Seat.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateSeats_Should_ThrowFormatException_When_JsonRowsHaveDifferentLengths()
    {
        var screen = new Screen
        {
            Id = Guid.CreateVersion7(),
            CinemaId = Guid.CreateVersion7(),
            Code = "S3",
            Type = ScreenType.TwoD,
            IsActive = true
        };

        var act = () => screen.GenerateSeats("[[1,1],[1]]");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void GenerateSeats_Should_ThrowFormatException_When_StairColumnDoesNotSpanAllRows()
    {
        var screen = new Screen
        {
            Id = Guid.CreateVersion7(),
            CinemaId = Guid.CreateVersion7(),
            Code = "S4",
            Type = ScreenType.TwoD,
            IsActive = true
        };

        var act = () => screen.GenerateSeats("[[0,1],[1,1]]");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void GenerateSeats_Should_ThrowFormatException_When_CellValueIsOutOfRange()
    {
        var screen = new Screen
        {
            Id = Guid.CreateVersion7(),
            CinemaId = Guid.CreateVersion7(),
            Code = "S5",
            Type = ScreenType.TwoD,
            IsActive = true
        };

        var act = () => screen.GenerateSeats("[[4]]");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void GenerateSeats_Should_UseSweetPrefixForCode_When_SeatTypeIsCouple()
    {
        var screen = new Screen
        {
            Id = Guid.CreateVersion7(),
            CinemaId = Guid.CreateVersion7(),
            Code = "S6",
            Type = ScreenType.TwoD,
            IsActive = true
        };

        screen.GenerateSeats("[[3,0]]");

        screen.Seat.Should().ContainSingle();
        screen.Seat[0].Code.Should().StartWith("Sweet");
        screen.Seat[0].Type.Should().Be(SeatType.Couple);
    }
}
