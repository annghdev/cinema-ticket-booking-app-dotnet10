using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class ScreenRepository(AppDbContext db) : BaseRepository<Screen>(db), IScreenRepository
{
}
