namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Updates basic information for an existing cinema.
/// </summary>
public class UpdateCinemaBasicInfoCommand : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? Geo { get; set; }
    public string Address { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles cinema basic-info update requests.
/// </summary>
public class UpdateCinemaBasicInfoHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Updates basic information for an existing cinema and persists changes.
    /// </summary>
    public async Task Handle(UpdateCinemaBasicInfoCommand cmd, CancellationToken ct)
    {
        var cinema = await uow.Cinemas.GetByIdAsync(cmd.Id, ct);
        if (cinema is null)
        {
            throw new InvalidOperationException($"Cinema with ID '{cmd.Id}' not found.");
        }

        cinema.UpdateBasicInfo(
            cmd.Name,
            cmd.ThumbnailUrl,
            cmd.Geo,
            cmd.Address);

        uow.Cinemas.Update(cinema);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates basic-info update command payload.
/// </summary>
public class UpdateCinemaBasicInfoValidator : AbstractValidator<UpdateCinemaBasicInfoCommand>
{
    public UpdateCinemaBasicInfoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Cinema ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Cinema name is required.")
            .MaximumLength(MaxLengthConsts.Name)
                .WithMessage($"Cinema name cannot exceed {MaxLengthConsts.Name} characters.");

        RuleFor(x => x.ThumbnailUrl)
            .NotEmpty().WithMessage("Thumbnail URL is required.")
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                .WithMessage("Thumbnail URL must be a valid absolute URL.")
            .MaximumLength(MaxLengthConsts.Url)
                .WithMessage($"Thumbnail URL cannot exceed {MaxLengthConsts.Url} characters.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(MaxLengthConsts.Address)
                .WithMessage($"Address cannot exceed {MaxLengthConsts.Address} characters.");
    }
}
