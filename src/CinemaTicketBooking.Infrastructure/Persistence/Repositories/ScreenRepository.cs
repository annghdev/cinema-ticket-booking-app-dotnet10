using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class ScreenRepository(AppDbContext db) : BaseRepository<Screen>(db), IScreenRepository
{
    public Task<Screen?> GetByIdWithSeatsAsync(Guid id, CancellationToken ct = default)
    {
        return _dbSet
            .Include(x => x.Seats)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
