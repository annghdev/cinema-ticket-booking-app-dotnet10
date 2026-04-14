using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.UnitTests.Shared;

/// <summary>
/// En factory helpers for Domain entity unit tests.
/// </summary>
public static class DomainTestBuilders
{
    /// <summary>
    /// Returns a start time safely in the future for ShowTime.Create validation.
    /// </summary>
    public static DateTimeOffset FutureStart(int hoursFromNow = 2) =>
        DateTimeOffset.UtcNow.AddHours(hoursFromNow);

    /// <summary>
    /// Minimal movie eligible for scheduling.
    /// </summary>
    public static Movie MovieNowShowing(int durationMinutes = 90, string name = "Test Movie")
    {
        return new Movie
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Duration = durationMinutes,
            Status = MovieStatus.NowShowing
        };
    }

    /// <summary>
    /// Screen with a single-row JSON seat map: two regular seats and a stair column.
    /// </summary>
    public static Screen ScreenWithSeatMap(
        Guid cinemaId,
        ScreenType type = ScreenType.TwoD,
        string code = "S1",
        string seatMapJson = "[[1,1,0]]")
    {
        var screen = new Screen
        {
            Id = Guid.CreateVersion7(),
            CinemaId = cinemaId,
            Code = code,
            Type = type,
            IsActive = true,
            SeatMap = seatMapJson
        };
        screen.GenerateSeats(seatMapJson);
        return screen;
    }

    /// <summary>
    /// Active pricing policies covering Regular and VIP for the given screen type.
    /// </summary>
    public static List<PricingPolicy> PoliciesForRegularAndVip(
        Guid? cinemaId,
        ScreenType screenType,
        decimal baseRegular = 50_000m,
        decimal baseVip = 100_000m,
        decimal coefficient = 1.0m)
    {
        return
        [
            new PricingPolicy
            {
                Id = Guid.CreateVersion7(),
                CinemaId = cinemaId,
                ScreenType = screenType,
                SeatType = SeatType.Regular,
                BasePrice = baseRegular,
                ScreenCoefficient = coefficient,
                IsActive = true
            },
            new PricingPolicy
            {
                Id = Guid.CreateVersion7(),
                CinemaId = cinemaId,
                ScreenType = screenType,
                SeatType = SeatType.VIP,
                BasePrice = baseVip,
                ScreenCoefficient = coefficient,
                IsActive = true
            }
        ];
    }

    /// <summary>
    /// Single active policy for one seat type.
    /// </summary>
    public static PricingPolicy Policy(
        SeatType seatType,
        ScreenType screenType,
        Guid? cinemaId = null,
        decimal basePrice = 50_000m,
        decimal coefficient = 1.0m)
    {
        return new PricingPolicy
        {
            Id = Guid.CreateVersion7(),
            CinemaId = cinemaId,
            ScreenType = screenType,
            SeatType = seatType,
            BasePrice = basePrice,
            ScreenCoefficient = coefficient,
            IsActive = true
        };
    }

    /// <summary>
    /// Guest customer with session id for ticket lock matching.
    /// </summary>
    public static Customer GuestCustomer(string sessionId = "session-1")
    {
        return new Customer
        {
            Id = Guid.CreateVersion7(),
            Name = "Guest",
            SessionId = sessionId,
            IsRegistered = false
        };
    }

    /// <summary>
    /// Pending booking shell; set ShowTimeId before AddTicket.
    /// </summary>
    public static Booking PendingBooking(Guid showTimeId, Customer customer, string customerName = "Booker")
    {
        return new Booking
        {
            Id = Guid.CreateVersion7(),
            ShowTimeId = showTimeId,
            Customer = customer,
            CustomerId = customer.Id,
            CustomerName = customerName,
            Status = BookingStatus.Pending,
            OriginAmount = 0,
            FinalAmount = 0
        };
    }

    /// <summary>
    /// Ticket in Locking state for a showtime, locked by session or customer id string.
    /// </summary>
    public static Ticket LockingTicket(
        Guid showTimeId,
        decimal price,
        string lockedBy,
        string code = "T1",
        DateTimeOffset? lockExpiresAt = null)
    {
        return new Ticket
        {
            Id = Guid.CreateVersion7(),
            ShowTimeId = showTimeId,
            Code = code,
            Price = price,
            Status = TicketStatus.Locking,
            LockingBy = lockedBy,
            LockExpiresAt = lockExpiresAt ?? DateTimeOffset.UtcNow.AddMinutes(5)
        };
    }

    /// <summary>
    /// Ticket in PendingPayment state for a showtime and booking.
    /// </summary>
    public static Ticket PendingPaymentTicket(
        Guid showTimeId,
        Guid bookingId,
        decimal price,
        DateTimeOffset? paymentExpiresAt = null,
        string code = "T1")
    {
        return new Ticket
        {
            Id = Guid.CreateVersion7(),
            ShowTimeId = showTimeId,
            Code = code,
            Price = price,
            Status = TicketStatus.PendingPayment,
            BookingId = bookingId,
            PaymentExpiresAt = paymentExpiresAt ?? DateTimeOffset.UtcNow.AddMinutes(15)
        };
    }
}
