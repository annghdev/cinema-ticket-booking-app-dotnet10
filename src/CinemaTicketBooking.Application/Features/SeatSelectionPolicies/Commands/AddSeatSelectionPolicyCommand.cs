using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Domain;
using FluentValidation;

namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Adds a new seat selection policy.
/// </summary>
public class AddSeatSelectionPolicyCommand : ICommand
{
    public required string Name { get; set; }
    public bool IsGlobalDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public int MaxTicketsPerCheckout { get; set; } = 8;
    public int MaxRowsPerCheckout { get; set; } = 2;
    public SeatSelectionPolicyLevel OrphanSeatLevel { get; set; } = SeatSelectionPolicyLevel.Block;
    public SeatSelectionPolicyLevel CheckerboardLevel { get; set; } = SeatSelectionPolicyLevel.Block;
    public SeatSelectionPolicyLevel SplitAcrossAisleLevel { get; set; } = SeatSelectionPolicyLevel.Block;
    public SeatSelectionPolicyLevel IsolatedRowEndSingleLevel { get; set; } = SeatSelectionPolicyLevel.Warning;
    public SeatSelectionPolicyLevel MisalignedRowsLevel { get; set; } = SeatSelectionPolicyLevel.Block;
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles add-seat-selection-policy requests.
/// </summary>
public class AddSeatSelectionPolicyHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Creates a seat selection policy and persists changes.
    /// </summary>
    public async Task<Guid> Handle(AddSeatSelectionPolicyCommand cmd, CancellationToken ct)
    {
        var policy = new SeatSelectionPolicy
        {
            Name = cmd.Name,
            IsGlobalDefault = cmd.IsGlobalDefault,
            IsActive = cmd.IsActive,
            MaxTicketsPerCheckout = cmd.MaxTicketsPerCheckout,
            MaxRowsPerCheckout = cmd.MaxRowsPerCheckout,
            OrphanSeatLevel = cmd.OrphanSeatLevel,
            CheckerboardLevel = cmd.CheckerboardLevel,
            SplitAcrossAisleLevel = cmd.SplitAcrossAisleLevel,
            IsolatedRowEndSingleLevel = cmd.IsolatedRowEndSingleLevel,
            MisalignedRowsLevel = cmd.MisalignedRowsLevel
        };

        uow.SeatSelectionPolicies.Add(policy);
        await uow.CommitAsync(ct);
        return policy.Id;
    }
}

/// <summary>
/// Validates add-seat-selection-policy command payload.
/// </summary>
public class AddSeatSelectionPolicyValidator : AbstractValidator<AddSeatSelectionPolicyCommand>
{
    public AddSeatSelectionPolicyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255).WithMessage("Name must not exceed 255 characters.");
        RuleFor(x => x.MaxTicketsPerCheckout).GreaterThan(0).WithMessage("Max tickets must be positive.");
        RuleFor(x => x.MaxRowsPerCheckout).GreaterThan(0).WithMessage("Max rows must be positive.");
        RuleFor(x => x.OrphanSeatLevel).IsInEnum();
        RuleFor(x => x.CheckerboardLevel).IsInEnum();
        RuleFor(x => x.SplitAcrossAisleLevel).IsInEnum();
        RuleFor(x => x.IsolatedRowEndSingleLevel).IsInEnum();
        RuleFor(x => x.MisalignedRowsLevel).IsInEnum();
    }
}
