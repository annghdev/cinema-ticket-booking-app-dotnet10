namespace CinemaTicketBooking.Domain;

public interface IShowTimeRepository : IRepository<ShowTime>
{
    Task<Booking?> LoadFullAsync(Guid id, CancellationToken ct = default);
}
