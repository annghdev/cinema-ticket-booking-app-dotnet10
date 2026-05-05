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
            SupportedFormats = [ScreenType.TwoD],
            IsActive = true
        };

        screen.GenerateSeats("[[1,1,0]]");

        screen.Seats.Should().HaveCount(2);
        screen.Seats.Select(s => s.Code).Should().Contain("A1", "A2");
        screen.Seats.Should().OnlyContain(s => s.Type == SeatType.Regular);
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
            SupportedFormats = [ScreenType.TwoD],
            IsActive = true
        };

        var plain = "1 1 0\n1 1 0";
        screen.GenerateSeats(plain);

        screen.Seats.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateSeats_Should_ThrowFormatException_When_JsonRowsHaveDifferentLengths()
    {
        var screen = new Screen
        {
            Id = Guid.CreateVersion7(),
            CinemaId = Guid.CreateVersion7(),
            Code = "S3",
            SupportedFormats = [ScreenType.TwoD],
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
            SupportedFormats = [ScreenType.TwoD],
            IsActive = true
        };

        var act = () => screen.GenerateSeats("[[0,1],[1,1]]");
        act.Should().Throw<FormatException>();
    }

    /// <summary>
    /// SeatMap encoding: 0=Aisle, 1=Regular, 2=VIP, 3=SweetBox, 4=SweetBoxGap (couple spacer).
    /// Any value outside [0..4] must be rejected.
    /// Note: value 4 is now valid (SweetBox couple gap spacer) — use 5 as the out-of-range probe.
    /// </summary>
    [Fact]
    public void GenerateSeats_Should_ThrowFormatException_When_CellValueIsOutOfRange()
    {
        var screen = new Screen
        {
            Id = Guid.CreateVersion7(),
            CinemaId = Guid.CreateVersion7(),
            Code = "S5",
            SupportedFormats = [ScreenType.TwoD],
            IsActive = true
        };

        var act = () => screen.GenerateSeats("[[5]]");
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
            SupportedFormats = [ScreenType.TwoD],
            IsActive = true
        };

        screen.GenerateSeats("[[4,3,0]]");

        screen.Seats.Should().ContainSingle();
        screen.Seats[0].Code.Should().StartWith("Sweet");
        screen.Seats[0].Type.Should().Be(SeatType.Couple);
    }
}
