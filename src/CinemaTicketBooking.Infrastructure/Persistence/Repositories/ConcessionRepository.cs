using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class ConcessionRepository(AppDbContext db) : BaseRepository<Concession>(db), IConcessionRepository
{
}
