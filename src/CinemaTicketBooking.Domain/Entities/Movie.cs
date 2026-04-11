namespace CinemaTicketBooking.Domain;

public class Movie : AuditableEntity
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Studio { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string? OfficialTrailerUrl { get; set; }
    public int Duration { get; set; } // Minutes
    public MovieGenre Genre { get; set; }
    public MovieStatus Status { get; set; }
}

//public enum MovieStatus
//{
//    Ongoing,
//    NowShowing,
//    NoShow
//}

//public enum MovieGenre
//{
//    Action,
//    Comedy,
//    Drama,
//    Horror,
//    SciFi,
//    Romance,
//    Thriller,
//    Animation,
//    Documentary
//}