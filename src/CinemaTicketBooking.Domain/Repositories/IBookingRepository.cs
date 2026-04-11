namespace CinemaTicketBooking.Domain;

public interface IBookingRepository : IRepository<Booking>
{
    Task<Booking?> LoadFullAsync(Guid id, CancellationToken ct = default);
}
