using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class CustomerRepository(AppDbContext db) : BaseRepository<Customer>(db), ICustomerRepository
{
    public Task<Customer?> GetTrackedBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.SessionId == sessionId, cancellationToken);
    }
}
