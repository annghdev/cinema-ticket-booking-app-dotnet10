using CinemaTicketBooking.Domain.Repositories;
using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Infrastructure.Persistence.Repositories;

public class SlideRepository(AppDbContext context) : BaseRepository<Slide>(context), ISlideRepository
{
}
