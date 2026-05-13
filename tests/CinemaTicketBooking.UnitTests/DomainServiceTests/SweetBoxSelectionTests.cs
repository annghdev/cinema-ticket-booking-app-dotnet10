using CinemaTicketBooking.Domain;
using CinemaTicketBooking.UnitTests.Shared;
using FluentAssertions;

namespace CinemaTicketBooking.UnitTests.DomainServiceTests;

/// <summary>
/// Unit tests for SweetBox couple-seat selection — specifically the SeatMap value 4
/// (SweetBox couple gap spacer) and the SplitAcrossAisle / IsolatedRowEndSingle rules.
/// </summary>
public class SweetBoxSelectionTests
{
    // =============================================
    // Standard screen helpers (mirrors seed screen1 / showtime1)
    // =============================================

    private const string StandardSeatMap =
        "[[1,1,0,2,2,2,1,0]," +
        "[1,1,0,2,2,2,1,0]," +
        "[1,1,0,2,2,2,1,0]," +
        "[1,1,0,2,2,2,1,0]," +
        "[4,3,0,4,3,4,3,0]]";

    private const string CompactSeatMap =
        "[[1,1,0,2,2,1,0]," +
        "[1,1,0,2,2,1,0]," +
        "[1,1,0,2,2,1,0]," +
        "[4,3,0,3,4,3,0]]";

    // =============================================
    // SeatMap value-4 encoding: GenerateSeats behaviour
    // =============================================

    /// <summary>
    /// Value 4 cells must be skipped by GenerateSeats (no Seat entity created for them).
    /// The two flanking SweetBox seats must still be created with adjacent seat codes.
    /// </summary>
    [Fact]
    public void GenerateSeats_Should_SkipSweetBoxGapCell_AndCreateBothSweetBoxSeats()
    {
        // Arrange – single row with two SweetBox seats flanking a gap spacer.
        var screen = BuildScreen("[[3,4,3,0]]");

        // Assert – only Sweet1 and Sweet2 exist; no seat for the gap (col 2) or the aisle (col 4).
        screen.Seats.Should().HaveCount(2);
        screen.Seats.Should().Contain(s => s.Code == "Sweet1");
        screen.Seats.Should().Contain(s => s.Code == "Sweet2");
    }

    /// <summary>
    /// Confirms that adjacent SweetBox seats (gap spacer between them) receive consecutive
    /// column values that differ by exactly 2, not 1, because the gap occupies col+1.
    /// </summary>
    [Fact]
    public void GenerateSeats_SweetBoxPair_Columns_Are_Separated_By_Gap_Cell()
    {
        // Arrange – layout: col4=SweetBox(Sweet1), col3=gap, col2=SweetBox(Sweet2), col1=aisle
        var screen = BuildScreen("[[0,3,4,3]]");

        var sweet1 = screen.Seats.Single(s => s.Code == "Sweet1");
        var sweet2 = screen.Seats.Single(s => s.Code == "Sweet2");

        // Sweet1 is the rightmost non-gap cell (col 4), Sweet2 is the next one (col 2).
        sweet1.Column.Should().Be(4);
        sweet2.Column.Should().Be(2);
        (sweet1.Column - sweet2.Column).Should().Be(2, "gap spacer sits between them");
    }

    // =============================================
    // SplitAcrossAisleRule: SweetBox couple seats (standard screen / showtime1)
    // =============================================

    /// <summary>
    /// Reproduces the original bug: selecting Sweet1+Sweet2 on the standard screen used to
    /// raise SplitAcrossAisle because the gap spacer (value 4) was treated as an aisle.
    /// After the fix, both seats are in the same aisle segment and validation must pass.
    /// </summary>
    [Fact]
    public void Validate_Should_Pass_When_SweetBox_Couple_Selected_On_StandardScreen()
    {
        // Arrange
        var screen = BuildScreen(StandardSeatMap);
        var showTime = BuildShowTime(screen,
            ("Sweet1", TicketStatus.Locking, "session-1"),
            ("Sweet2", TicketStatus.Locking, "session-1"));
        var policy = SeatSelectionPolicy.CreateDefault();
        var selectedIds = showTime.Tickets.Select(t => t.Id).ToList();

        // Act
        var result = SeatSelectionValidator.CreateDefault().Validate(showTime, policy, selectedIds, "session-1");

        // Assert
        result.CanProceed.Should().BeTrue("Sweet1 and Sweet2 are adjacent couple seats with no real aisle between them");
        result.Errors.Should().NotContain(v => v.Type == SeatSelectionViolationType.SplitAcrossAisle);
    }

    /// <summary>
    /// Selecting Sweet1 alone must also be valid (no aisle split, no orphan).
    /// </summary>
    [Fact]
    public void Validate_Should_Pass_When_Single_SweetBox_Selected()
    {
        var screen = BuildScreen(StandardSeatMap);
        var showTime = BuildShowTime(screen, ("Sweet1", TicketStatus.Locking, "session-1"));
        var policy = SeatSelectionPolicy.CreateDefault();
        var selectedIds = showTime.Tickets.Select(t => t.Id).ToList();

        var result = SeatSelectionValidator.CreateDefault().Validate(showTime, policy, selectedIds, "session-1");

        result.Errors.Should().NotContain(v => v.Type == SeatSelectionViolationType.SplitAcrossAisle);
    }

    /// <summary>
    /// Selecting a SweetBox seat and a regular seat separated by a real aisle (value 0)
    /// must still raise SplitAcrossAisle (real aisle, not a gap spacer).
    /// Note: in row [[1,0,3]], right-to-left scan gives Sweet1(col3,seatNum=1) then A2(col1,seatNum=2).
    /// The regular seat code is A2, not A1, because seatNumber is shared across all seat types in a row.
    /// </summary>
    [Fact]
    public void Validate_Should_Block_When_SweetBox_And_RegularSeat_Span_RealAisle()
    {
        // Row scan (right→left): col3=SweetBox→Sweet1(seatNum=1), col2=aisle(skip), col1=Regular→A2(seatNum=2).
        var screen = BuildScreen("[[1,0,3]]");
        var showTime = BuildShowTime(screen,
            ("A2", TicketStatus.Locking, "session-1"),
            ("Sweet1", TicketStatus.Locking, "session-1"));
        var policy = SeatSelectionPolicy.CreateDefault();
        var selectedIds = showTime.Tickets.Select(t => t.Id).ToList();

        var result = SeatSelectionValidator.CreateDefault().Validate(showTime, policy, selectedIds, "session-1");

        result.CanProceed.Should().BeFalse();
        result.Errors.Should().Contain(v => v.Type == SeatSelectionViolationType.SplitAcrossAisle);
    }

    // =============================================
    // SplitAcrossAisleRule: compact screen / showtime2
    // =============================================

    /// <summary>
    /// Reproduces the second reported bug: selecting Sweet1+Sweet2 on the compact screen
    /// (previously [3,0,0,3,0,3,0]) raised SplitAcrossAisle. With the corrected seed map
    /// [0,0,0,3,4,3,0] and value 4 transparent to the validator, the rule must not fire.
    /// </summary>
    [Fact]
    public void Validate_Should_Pass_When_SweetBox_Couple_Selected_On_CompactScreen()
    {
        var screen = BuildScreen(CompactSeatMap);
        var showTime = BuildShowTime(screen,
            ("Sweet1", TicketStatus.Locking, "session-1"),
            ("Sweet2", TicketStatus.Locking, "session-1"));
        var policy = SeatSelectionPolicy.CreateDefault();
        var selectedIds = showTime.Tickets.Select(t => t.Id).ToList();

        var result = SeatSelectionValidator.CreateDefault().Validate(showTime, policy, selectedIds, "session-1");

        result.CanProceed.Should().BeTrue();
        result.Errors.Should().NotContain(v => v.Type == SeatSelectionViolationType.SplitAcrossAisle);
    }


    // =============================================
    // IsolatedRowEndSingle: no false positive from unrelated rows
    // =============================================

    /// <summary>
    /// Selecting Sweet1+Sweet2 must not trigger IsolatedRowEndSingle on a completely
    /// different row that is unaffected by the selection (A4 false positive from bug report).
    /// </summary>
    [Fact]
    public void Validate_Should_Not_Produce_IsolatedRowEndSingle_OnUnrelatedRow_When_SweetBoxCouple_Selected()
    {
        // Row 1: [1,1,1] — three regular seats A3(col1), A2(col2), A1(col3); one segment, none occupied.
        // Row 2: [3,4,3] — Sweet2(col1) | gap(col2) | Sweet1(col3); no stair columns to conflict.
        // No row-1 seat is occupied, so IsolatedRowEndSingleRule cannot fire for row 1.
        const string seatMap = "[[1,1,1],[3,4,3]]";
        var screen = BuildScreen(seatMap);
        var showTime = BuildShowTime(screen,
            ("Sweet1", TicketStatus.Locking, "session-1"),
            ("Sweet2", TicketStatus.Locking, "session-1"));
        var policy = SeatSelectionPolicy.CreateDefault();
        var selectedIds = showTime.Tickets.Select(t => t.Id).ToList();

        var result = SeatSelectionValidator.CreateDefault().Validate(showTime, policy, selectedIds, "session-1");

        result.Warnings.Should().NotContain(v =>
            v.Type == SeatSelectionViolationType.IsolatedRowEndSingle,
            "no row-1 seat is occupied so no isolated edge seat can form");
        result.Errors.Should().NotContain(v => v.Type == SeatSelectionViolationType.SplitAcrossAisle);
    }

    /// <summary>
    /// IsolatedRowEndSingle must still fire correctly for a normal row when a selection
    /// genuinely leaves an isolated seat at the row end (regression guard).
    /// </summary>
    [Fact]
    public void Validate_Should_WarnIsolatedRowEndSingle_When_SelectionLeavesEdgeSeat()
    {
        // Row: A1 A2 A3 A4 A5 — selecting A2..A4 leaves A1 and A5 isolated at each end.
        var screen = BuildScreen("[[1,1,1,1,1]]");
        var showTime = BuildShowTime(screen,
            ("A2", TicketStatus.Locking, "session-1"),
            ("A3", TicketStatus.Locking, "session-1"),
            ("A4", TicketStatus.Locking, "session-1"));
        var policy = SeatSelectionPolicy.CreateDefault();
        var selectedIds = showTime.Tickets.Select(t => t.Id).ToList();

        var result = SeatSelectionValidator.CreateDefault().Validate(showTime, policy, selectedIds, "session-1");

        result.Warnings.Should().Contain(v => v.Type == SeatSelectionViolationType.IsolatedRowEndSingle);
    }

    // =============================================
    // SeatMap encoding validation
    // =============================================

    /// <summary>
    /// A SeatMap containing value 4 must be accepted by Screen.GenerateSeats without throwing.
    /// </summary>
    [Fact]
    public void GenerateSeats_Should_Accept_SeatMap_With_SweetBoxGapValue()
    {
        var act = () => BuildScreen("[[3,4,3,0,0]]");
        act.Should().NotThrow();
    }

    /// <summary>
    /// A SeatMap containing a value greater than 4 must be rejected.
    /// </summary>
    [Fact]
    public void GenerateSeats_Should_Reject_SeatMap_With_InvalidValue()
    {
        var act = () => BuildScreen("[[1,5,1]]");
        act.Should().Throw<FormatException>();
    }

    // =============================================
    // Shared test helpers (local to this fixture)
    // =============================================

    private static Screen BuildScreen(string seatMap)
    {
        var screen = new Screen
        {
            Id = Guid.CreateVersion7(),
            CinemaId = Guid.CreateVersion7(),
            Code = "TEST",
            SupportedFormats = [ScreenType.TwoD],
            IsActive = true,
            SeatMap = seatMap
        };
        screen.GenerateSeats(seatMap);
        return screen;
    }

    private static ShowTime BuildShowTime(
        Screen screen,
        params (string SeatCode, TicketStatus Status, string? LockingBy)[] seats)
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

        foreach (var item in seats)
        {
            showTime.Tickets.Add(new Ticket
            {
                Id = Guid.CreateVersion7(),
                ShowTimeId = showTime.Id,
                SeatId = screen.Seats.FirstOrDefault(x => x.Code == item.SeatCode)?.Id,
                SeatCode = item.SeatCode,
                Status = item.Status,
                LockingBy = item.LockingBy,
                Code = $"TEST-{item.SeatCode}",
                Description = item.SeatCode,
                Price = 100_000m
            });
        }

        return showTime;
    }
}
