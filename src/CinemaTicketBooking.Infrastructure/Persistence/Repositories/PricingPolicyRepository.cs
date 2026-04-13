using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class PricingPolicyRepository(AppDbContext db)
    : BaseRepository<PricingPolicy>(db), IPricingPolicyRepository
{
    public Task<List<PricingPolicy>> GetActivePoliciesAsync(
        Guid cinemaId, ScreenType screenType, CancellationToken ct = default)
    {
        return _dbSet.Where(p => p.CinemaId == cinemaId && p.ScreenType == screenType && p.IsActive)
                     .ToListAsync(ct);
    }

    public Task<PricingPolicy?> GetActivePolicyAsync(
        Guid cinemaId,
        ScreenType screenType,
        SeatType seatType,
        CancellationToken ct = default)
    {
        return _dbSet.FirstOrDefaultAsync(p =>
                        p.CinemaId == cinemaId
                        && p.ScreenType == screenType
                        && p.SeatType == seatType
                        && p.IsActive, ct);
    }
}
