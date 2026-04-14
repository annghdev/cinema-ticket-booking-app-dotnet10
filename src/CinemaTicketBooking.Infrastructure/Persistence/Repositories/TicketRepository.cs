using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class TicketRepository : BaseRepository<Ticket>, ITicketRepository
{
    private readonly AppDbContext _db;

    public TicketRepository(AppDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<bool> TryAcquireLockGuardAsync(Guid ticketId, CancellationToken ct = default)
    {
        var lockKey = ticketId.ToString("N");
        return await _db.Database
            .SqlQuery<bool>($"select pg_try_advisory_xact_lock(hashtext({lockKey})) as \"Value\"")
            .SingleAsync(ct);
    }
}
