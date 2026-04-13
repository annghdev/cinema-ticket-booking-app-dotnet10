using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class PricingPolicyRepository(AppDbContext db)
    : BaseRepository<PricingPolicy>(db), IPricingPolicyRepository
{
    public Task<List<PricingPolicy>> GetActivePoliciesAsync(
        Guid cinemaId, ScreenType screenType, CancellationToken ct = default)
    {
        return _dbSet
            .Where(p => p.ScreenType == screenType
                        && p.IsActive
                        && (p.CinemaId == cinemaId || p.CinemaId == null))
            .OrderByDescending(p => p.CinemaId == cinemaId)
            .ToListAsync(ct);
    }

    public Task<PricingPolicy?> GetActivePolicyAsync(
        Guid cinemaId,
        ScreenType screenType,
        SeatType seatType,
        CancellationToken ct = default)
    {
        return _dbSet
            .Where(p => p.ScreenType == screenType
                        && p.SeatType == seatType
                        && p.IsActive
                        && (p.CinemaId == cinemaId || p.CinemaId == null))
            .OrderByDescending(p => p.CinemaId == cinemaId)
            .FirstOrDefaultAsync(ct);
    }
}
