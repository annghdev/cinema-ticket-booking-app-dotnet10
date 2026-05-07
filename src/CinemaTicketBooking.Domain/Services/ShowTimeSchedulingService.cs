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
        ScreenType? format = null,
        CancellationToken ct = default)
    {
        // 1. Resolve format and load pricing policies
        // If a format is provided, we strictly use it.
        // If not, we "automatically" pick the first supported format from the screen that has defined pricing policies.
        
        ScreenType resolvedFormat;
        List<PricingPolicy> pricingPolicies;

        if (format.HasValue)
        {
            resolvedFormat = format.Value;
            pricingPolicies = await _pricingPolicyRepository.GetActivePoliciesAsync(screen.CinemaId, resolvedFormat, ct);

            if (pricingPolicies.Count == 0)
                throw new InvalidOperationException(
                    $"No pricing policies found for requested Format '{resolvedFormat}' (Cinema: {screen.CinemaId}).");
        }
        else
        {
            // Automatic resolution logic
            pricingPolicies = [];
            resolvedFormat = screen.Type; // Fallback if no supported formats or policies found
            bool found = false;

            foreach (var supportedFormat in screen.SupportedFormats)
            {
                var policies = await _pricingPolicyRepository.GetActivePoliciesAsync(screen.CinemaId, supportedFormat, ct);
                if (policies.Count > 0)
                {
                    resolvedFormat = supportedFormat;
                    pricingPolicies = policies;
                    found = true;
                    break;
                }
            }

            if (!found)
                throw new InvalidOperationException(
                    $"No pricing policies found for any supported format of Screen '{screen.Code}' (Cinema: {screen.CinemaId}, Formats: [{string.Join(", ", screen.SupportedFormats)}]).");
        }

        // 2. Create ShowTime (entity validates invariants + generates priced tickets)
        var newShowTime = ShowTime.Create(movie, screen, startAt, pricingPolicies, resolvedFormat);

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
