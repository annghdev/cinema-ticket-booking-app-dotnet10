namespace CinemaTicketBooking.Domain;

public class Screen : AuditableEntity
{
    public Guid CinemaId { get; set; }
    public Cinema? Cinema { get; set; }
    public required string Code { get; set; }
    public int RowOfSeats { get; set; }
    public int ColumnOfSeats { get; set; }
    public int TotalSeats { get; set; }
    public string SeatMap { get; set; } = string.Empty;
    public ScreenType Type { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Seat> Seat { get; set; } = [];
}

public class Seat : IEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Row { get; set; }
    public int Column { get; set; }
    public bool IsAvailable { get; set; }
    public SeatType Type { get; set; }
    public bool IsActive { get; set; } = true;
}

//public enum ScreenType
//{
//    TwoD,
//    ThreeD,
//    IMAX
//}

//public enum SeatType
//{
//    Regular,
//    VIP,
//    Couple,
//}