using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class SeatSelectionPolicyRepository(AppDbContext db)
    : BaseRepository<SeatSelectionPolicy>(db), ISeatSelectionPolicyRepository
{
    public Task<SeatSelectionPolicy?> GetActiveGlobalAsync(CancellationToken ct = default)
    {
        return _dbSet
            .Where(x => x.IsActive && x.IsGlobalDefault)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }
}
