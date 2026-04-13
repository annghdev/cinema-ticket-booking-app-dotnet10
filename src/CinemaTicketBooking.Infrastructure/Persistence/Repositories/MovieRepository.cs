using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class MovieRepository(AppDbContext db) : BaseRepository<Movie>(db), IMovieRepository
{
}
