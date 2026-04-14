using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Gets a pricing policy by id.
/// </summary>
public class GetPricingPolicyByIdQuery : IQuery
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles query for a specific pricing policy.
/// </summary>
public class GetPricingPolicyByIdHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Returns pricing policy data when found; otherwise null.
    /// </summary>
    public async Task<PricingPolicyDto?> Handle(GetPricingPolicyByIdQuery query, CancellationToken ct)
    {
        var item = await uow.PricingPolicies
            .GetQueryFilter()
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(x => new PricingPolicyDto(
                x.Id,
                x.CinemaId,
                x.ScreenType,
                x.SeatType,
                x.BasePrice,
                x.ScreenCoefficient,
                x.BasePrice * x.ScreenCoefficient,
                x.IsActive,
                x.CreatedAt))
            .FirstOrDefaultAsync(ct);

        return item;
    }
}

/// <summary>
/// Validates query payload.
/// </summary>
public class GetPricingPolicyByIdValidator : AbstractValidator<GetPricingPolicyByIdQuery>
{
    public GetPricingPolicyByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Pricing policy ID is required.");
    }
}
