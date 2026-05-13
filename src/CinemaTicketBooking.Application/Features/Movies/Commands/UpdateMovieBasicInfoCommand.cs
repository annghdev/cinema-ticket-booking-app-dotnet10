namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Updates basic information for an existing movie.
/// </summary>
public class UpdateMovieBasicInfoCommand : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Studio { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string? OfficialTrailerUrl { get; set; }
    public int Duration { get; set; }
    public MovieGenre Genre { get; set; }
    public decimal TargetReach { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles movie basic-info update requests.
/// </summary>
public class UpdateMovieBasicInfoCommandHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Updates basic information for an existing movie and persists changes.
    /// </summary>
    public async Task Handle(UpdateMovieBasicInfoCommand command, CancellationToken ct)
    {
        var movie = await uow.Movies.GetByIdAsync(command.Id, ct);
        if (movie is null)
        {
            throw new InvalidOperationException($"Movie with ID '{command.Id}' not found.");
        }

        movie.UpdateBasicInfo(
            command.Name,
            command.Description,
            command.ThumbnailUrl,
            command.Studio,
            command.Director,
            command.OfficialTrailerUrl,
            command.Duration,
            command.Genre,
            command.TargetReach);

        uow.Movies.Update(movie);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates basic-info update command payload.
/// </summary>
public class UpdateMovieBasicInfoValidator : AbstractValidator<UpdateMovieBasicInfoCommand>
{
    public UpdateMovieBasicInfoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Movie ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Movie name is required.")
            .MaximumLength(MaxLengthConsts.Name)
            .WithMessage($"Movie name cannot exceed {MaxLengthConsts.Name} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(MaxLengthConsts.Description)
            .WithMessage($"Movie description cannot exceed {MaxLengthConsts.Description} characters.");

        RuleFor(x => x.ThumbnailUrl)
            .NotEmpty().WithMessage("Thumbnail URL is required.")
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("Thumbnail URL must be a valid absolute URL.")
            .MaximumLength(MaxLengthConsts.Url)
            .WithMessage($"Thumbnail URL cannot exceed {MaxLengthConsts.Url} characters.");

        RuleFor(x => x.Studio)
            .NotEmpty().WithMessage("Studio is required.")
            .MaximumLength(MaxLengthConsts.Name)
            .WithMessage($"Studio cannot exceed {MaxLengthConsts.Name} characters.");

        RuleFor(x => x.Director)
            .NotEmpty().WithMessage("Director is required.")
            .MaximumLength(MaxLengthConsts.ActorName)
            .WithMessage($"Director cannot exceed {MaxLengthConsts.ActorName} characters.");

        RuleFor(x => x.OfficialTrailerUrl)
            .Must(url => string.IsNullOrWhiteSpace(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Official trailer URL must be a valid absolute URL.");

        RuleFor(x => x.OfficialTrailerUrl)
            .MaximumLength(MaxLengthConsts.Url)
            .When(x => !string.IsNullOrWhiteSpace(x.OfficialTrailerUrl))
            .WithMessage($"Official trailer URL cannot exceed {MaxLengthConsts.Url} characters.");

        RuleFor(x => x.Duration)
            .InclusiveBetween(1, 600)
            .WithMessage("Duration must be between 1 and 600 minutes.");

        RuleFor(x => x.Genre)
            .IsInEnum().WithMessage("Invalid movie genre.");

        RuleFor(x => x.TargetReach)
            .GreaterThanOrEqualTo(0).WithMessage("Target reach cannot be negative.");
    }
}
