using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class BookingRepository(AppDbContext db) : BaseRepository<Booking>(db), IBookingRepository
{
    public Task<Booking?> LoadFullAsync(Guid id, CancellationToken ct = default)
    {
        return _dbSet
            .Include(b => b.Tickets)
                .ThenInclude(bt => bt.Ticket)
            .Include(b => b.Concessions)
                .ThenInclude(bc => bc.Concession)
            .Include(b => b.ShowTime)
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }
}
