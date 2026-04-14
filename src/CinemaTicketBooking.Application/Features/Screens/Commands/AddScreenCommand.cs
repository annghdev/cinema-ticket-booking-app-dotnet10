using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Adds a new screen into a cinema.
/// </summary>
public class AddScreenCommand : ICommand
{
    public Guid CinemaId { get; set; }
    public required string Code { get; set; }
    public int RowOfSeats { get; set; }
    public int ColumnOfSeats { get; set; }
    public int TotalSeats { get; set; }
    public string SeatMap { get; set; } = string.Empty;
    public ScreenType Type { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles add-screen requests.
/// </summary>
public class AddScreenHandler(IUnitOfWork uow)
{
    public async Task<Guid> Handle(AddScreenCommand cmd, CancellationToken ct)
    {
        var screen = MapCommandToEntity(cmd);
        screen.GenerateSeats(cmd.SeatMap);
        uow.Screens.Add(screen);
        await uow.CommitAsync(ct);
        return screen.Id;
    }

    private static Screen MapCommandToEntity(AddScreenCommand cmd)
    {
        return Screen.Create(
            cinemaId: cmd.CinemaId,
            code: cmd.Code,
            rowOfSeats: cmd.RowOfSeats,
            columnOfSeats: cmd.ColumnOfSeats,
            totalSeats: cmd.TotalSeats,
            seatMap: cmd.SeatMap,
            type: cmd.Type);
    }
}

/// <summary>
/// Validates add-screen command payload.
/// </summary>
public class AddScreenValidator : AbstractValidator<AddScreenCommand>
{
    public AddScreenValidator()
    {
        RuleFor(x => x.CinemaId)
            .NotEmpty().WithMessage("Cinema ID is required.");

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
