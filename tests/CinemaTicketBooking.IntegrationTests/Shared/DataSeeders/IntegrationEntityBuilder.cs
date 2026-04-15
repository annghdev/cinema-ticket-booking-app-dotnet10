using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;

public static class IntegrationEntityBuilder
{
    public static Cinema Cinema(string name = "Cinema A")
    {
        return new Cinema
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Address = "123 Integration Street",
            Geo = "10.0,106.0",
            IsActive = true
        };
    }

    public static Movie Movie(string name = "Movie A", MovieStatus status = MovieStatus.NowShowing)
    {
        return new Movie
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = "Integration test movie",
            Duration = 120,
            Genre = MovieGenre.Action,
            Status = status
        };
    }

    public static Screen Screen(Guid cinemaId, string code = "S1", string seatMap = "[[1,1,0],[2,2,0]]")
    {
        var screen = new Screen
        {
            Id = Guid.CreateVersion7(),
            CinemaId = cinemaId,
            Code = code,
            SeatMap = seatMap,
            Type = ScreenType.TwoD,
            IsActive = true
        };

        screen.GenerateSeats(seatMap);
        screen.RowOfSeats = 2;
        screen.ColumnOfSeats = 3;
        screen.TotalSeats = screen.Seats.Count;
        return screen;
    }

    public static ShowTime ShowTime(Guid movieId, Guid screenId, ShowTimeStatus status = ShowTimeStatus.Upcoming)
    {
        var start = DateTimeOffset.UtcNow.AddHours(2);
        return new ShowTime
        {
            Id = Guid.CreateVersion7(),
            MovieId = movieId,
            ScreenId = screenId,
            Date = DateOnly.FromDateTime(start.DateTime),
            StartAt = start,
            EndAt = start.AddHours(2),
            Status = status
        };
    }

    public static Ticket Ticket(Guid showTimeId, string code = "T-001", TicketStatus status = TicketStatus.Available)
    {
        const string seatCode = "A1";
        return new Ticket
        {
            Id = Guid.CreateVersion7(),
            ShowTimeId = showTimeId,
            Code = code,
            SeatCode = seatCode,
            Description = $"{seatCode} - Regular",
            Price = 100_000m,
            Status = status
        };
    }

    public static Customer Customer(string sessionId = "session-1")
    {
        return new Customer
        {
            Id = Guid.CreateVersion7(),
            Name = "Guest",
            SessionId = sessionId,
            PhoneNumber = "0123456789",
            Email = "guest@example.com",
            IsRegistered = false
        };
    }

    public static Concession Concession(string name = "Popcorn", decimal price = 50_000m)
    {
        return new Concession
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Price = price,
            IsAvailable = true
        };
    }

    public static PricingPolicy PricingPolicy(
        Guid? cinemaId,
        ScreenType screenType = ScreenType.TwoD,
        SeatType seatType = SeatType.Regular,
        bool isActive = true)
    {
        return new PricingPolicy
        {
            Id = Guid.CreateVersion7(),
            CinemaId = cinemaId,
            ScreenType = screenType,
            SeatType = seatType,
            BasePrice = 100_000m,
            ScreenCoefficient = 1m,
            IsActive = isActive
        };
    }

    public static Booking Booking(Guid showTimeId, Guid customerId, string customerName = "Booker")
    {
        return new Booking
        {
            Id = Guid.CreateVersion7(),
            ShowTimeId = showTimeId,
            CustomerId = customerId,
            CustomerName = customerName,
            PhoneNumber = "0123456789",
            Email = "booker@example.com",
            OriginAmount = 0m,
            FinalAmount = 0m,
            Status = BookingStatus.Pending
        };
    }
}
