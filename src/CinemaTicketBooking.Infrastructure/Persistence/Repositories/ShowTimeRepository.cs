using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class ShowTimeRepository(AppDbContext db) : BaseRepository<ShowTime>(db), IShowTimeRepository
{
    public Task<List<ShowTime>> GetActiveByScreenAndDateRangeAsync(
        Guid screenId, DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        CancellationToken ct = default)
    {
        return _dbSet.Where(st => st.ScreenId == screenId
            && st.StartAt >= rangeStart
            && st.EndAt <= rangeEnd
            && (st.Status == ShowTimeStatus.Upcoming || st.Status == ShowTimeStatus.Showing))
            .ToListAsync(ct);
    }

    public Task<ShowTime?> LoadFullAsync(Guid id, CancellationToken ct = default)
    {
        return _dbSet.Include(st => st.Movie)
            .Include(st => st.Screen)
                .ThenInclude(s => s!.Cinema)
            .Include(st => st.Screen)
                .ThenInclude(s => s!.Seats)
            .Include(st => st.Tickets)
            .FirstOrDefaultAsync(st => st.Id == id, ct);
    }
}
