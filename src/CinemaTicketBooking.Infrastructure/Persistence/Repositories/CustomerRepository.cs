using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class CustomerRepository(AppDbContext db) : BaseRepository<Customer>(db), ICustomerRepository
{
}
