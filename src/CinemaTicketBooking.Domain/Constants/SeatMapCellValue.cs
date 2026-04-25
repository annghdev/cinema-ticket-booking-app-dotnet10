namespace CinemaTicketBooking.Domain;

/// <summary>
/// Integer cell values used in the SeatMap 2-D grid stored on <see cref="Screen"/>.
/// </summary>
/// <remarks>
/// The grid is serialised as a JSON array of arrays, e.g.
/// <c>[[1,1,0,2,2,2,1,0],[3,0,0,3,4,3,0,0]]</c>
/// </remarks>
public static class SeatMapCellValue
{
    /// <summary>Walking aisle column. Not a seat. Treated as a segment boundary by the validator.</summary>
    public const int Aisle = 0;

    /// <summary>Regular seat.</summary>
    public const int Regular = 1;

    /// <summary>VIP seat.</summary>
    public const int VIP = 2;

    /// <summary>SweetBox (couple) seat.</summary>
    public const int SweetBox = 3;

    /// <summary>
    /// Visual gap spacer between the two cells of a SweetBox couple seat.
    /// Not a seat and NOT an aisle boundary — the validator skips it when splitting rows
    /// into segments, so the two SweetBox seats on either side remain in the same segment.
    /// </summary>
    public const int SweetBoxGap = 4;
}
