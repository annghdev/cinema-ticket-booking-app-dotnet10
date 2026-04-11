namespace CinemaTicketBooking.Domain;

/// <summary>
/// Domain Service responsible for scheduling ShowTimes
/// with conflict detection and pricing policy resolution.
/// </summary>
public class ShowTimeSchedulingService
{
    private readonly IShowTimeRepository _showTimeRepository;
    private readonly IPricingPolicyRepository _pricingPolicyRepository;

    public ShowTimeSchedulingService(
        IShowTimeRepository showTimeRepository,
        IPricingPolicyRepository pricingPolicyRepository)
    {
        _showTimeRepository = showTimeRepository;
        _pricingPolicyRepository = pricingPolicyRepository;
    }

    /// <summary>
    /// Creates a new ShowTime ensuring no schedule conflicts on the same Screen
    /// and resolving pricing policies for ticket generation.
    /// </summary>
    public async Task<ShowTime> ScheduleAsync(
        Movie movie,
        Screen screen,
        DateTimeOffset startAt,
        CancellationToken ct = default)
    {
        // 1. Load pricing policies for this Cinema + ScreenType
        var pricingPolicies = await _pricingPolicyRepository
            .GetActivePoliciesAsync(screen.CinemaId, screen.Type, ct);

        if (pricingPolicies.Count == 0)
            throw new InvalidOperationException(
                $"No pricing policies found for Cinema '{screen.CinemaId}', ScreenType '{screen.Type}'.");

        // 2. Create ShowTime (entity validates invariants + generates priced tickets)
        var newShowTime = ShowTime.Create(movie, screen, startAt, pricingPolicies);

        // 3. Check for schedule conflicts on the same Screen
        //    Query a generous range to cover edge cases (midnight crossover, etc.)
        var rangeStart = newShowTime.StartAt.AddHours(-24);
        var rangeEnd = newShowTime.OccupiedUntil.AddHours(24);

        var existingShowTimes = await _showTimeRepository
            .GetActiveByScreenAndDateRangeAsync(screen.Id, rangeStart, rangeEnd, ct);

        var conflict = existingShowTimes.FirstOrDefault(s => newShowTime.ConflictsWith(s));
        if (conflict is not null)
        {
            throw new InvalidOperationException(
                $"Schedule conflict: Screen '{screen.Code}' is occupied from " +
                $"{conflict.StartAt:HH:mm} to {conflict.OccupiedUntil:HH:mm} " +
                $"(ShowTime ID: {conflict.Id}).");
        }

        return newShowTime;
    }
}
