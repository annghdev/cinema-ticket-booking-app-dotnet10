namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Updates basic information for an existing screen.
/// </summary>
public class UpdateScreenBasicInfoCommand : ICommand
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int RowOfSeats { get; set; }
    public int ColumnOfSeats { get; set; }
    public int TotalSeats { get; set; }
    public string SeatMap { get; set; } = string.Empty;
    public ScreenType Type { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles screen basic-info update requests.
/// </summary>
public class UpdateScreenBasicInfoHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Updates basic information for an existing screen and persists changes.
    /// </summary>
    public async Task Handle(UpdateScreenBasicInfoCommand cmd, CancellationToken ct)
    {
        var screen = await uow.Screens.GetByIdWithSeatsAsync(cmd.Id, ct);
        if (screen is null)
        {
            throw new InvalidOperationException($"Screen with ID '{cmd.Id}' not found.");
        }

        screen.UpdateBasicInfo(
            code: cmd.Code,
            rowOfSeats: cmd.RowOfSeats,
            columnOfSeats: cmd.ColumnOfSeats,
            totalSeats: cmd.TotalSeats,
            seatMap: cmd.SeatMap,
            type: cmd.Type);

        screen.GenerateSeats(cmd.SeatMap);
        uow.Screens.Update(screen);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates basic-info update command payload.
/// </summary>
public class UpdateScreenBasicInfoValidator : AbstractValidator<UpdateScreenBasicInfoCommand>
{
    public UpdateScreenBasicInfoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Screen ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Screen code is required.")
            .MaximumLength(MaxLengthConsts.ScreenCode)
            .WithMessage($"Screen code cannot exceed {MaxLengthConsts.ScreenCode} characters.");

        RuleFor(x => x.RowOfSeats)
            .GreaterThan(0).WithMessage("RowOfSeats must be greater than 0.");

        RuleFor(x => x.ColumnOfSeats)
            .GreaterThan(0).WithMessage("ColumnOfSeats must be greater than 0.");

        RuleFor(x => x.TotalSeats)
            .GreaterThan(0).WithMessage("TotalSeats must be greater than 0.");

        RuleFor(x => x.SeatMap)
            .NotEmpty().WithMessage("SeatMap is required.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid screen type.");
    }
}
