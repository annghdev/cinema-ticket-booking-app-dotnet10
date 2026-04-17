namespace CinemaTicketBooking.Application.Features;

public class BookingDetailsDto
{
    public Guid BookingId { get; set; }
    public ShowTimeInfo ShowTimeInfo { get; set; } = default!;
    public decimal OriginalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string CheckinQrCode { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<TicketInfo> Tickets { get; set; } = [];
    public List<ConcessionInfo> Concessions { get; set; } = [];
}

public record ShowTimeInfo(
    string Screen,
    string Movie,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt);


public record TicketInfo(string SeatCode, decimal Price);

public record ConcessionInfo(
    string Name,
    string ImageUrl,
    decimal Price,
    int Quantity,
    decimal Amount);


public class BookingMinimalInfoDto
{
    public Guid BookingId { get; set; }
    public ShowTimeInfo ShowTimeInfo { get; set; } = default!;
    public decimal FinalAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public BookingStatus Status { get; set; }
}