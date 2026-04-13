using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class TicketRepository(AppDbContext db) : BaseRepository<Ticket>(db), ITicketRepository
{
}
