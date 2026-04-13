using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class CinemaRepository(AppDbContext db) : BaseRepository<Cinema>(db), ICinemaRepository
{
}
