using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Application.Features;

public class CreateMovieCommand : ICommand
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Studio { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string? OfficialTrailerUrl { get; set; }
    public int Duration { get; set; }
    public MovieGenre Genre { get; set; }
    public MovieStatus Status { get; set; }
    public decimal TargetReach { get; set; }

    public string CorrelationId { get; set; } = string.Empty;
}

public class CreateMovieCommandHandler(IUnitOfWork uow)
{
    public async Task<Guid> Handle(CreateMovieCommand command, CancellationToken ct)
    {
        var movie = Movie.Create(
            command.Name,
            command.Description,
            command.ThumbnailUrl,
            command.Studio,
            command.Director,
            command.OfficialTrailerUrl,
            command.Duration,
            command.Genre,
            command.Status,
            command.TargetReach);

        uow.Movies.Add(movie);
        await uow.CommitAsync(ct);
        return movie.Id;
    }
}

public class CreateMovieValidator : AbstractValidator<CreateMovieCommand>
{
    public CreateMovieValidator()
    {
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
            .WithMessage("Official trailer URL must be a valid absolute URL.")
            .MaximumLength(MaxLengthConsts.Url)
            .When(x => !string.IsNullOrWhiteSpace(x.OfficialTrailerUrl))
            .WithMessage($"Official trailer URL cannot exceed {MaxLengthConsts.Url} characters.");

        RuleFor(x => x.Duration)
            .InclusiveBetween(1, 600)
            .WithMessage("Duration must be between 1 and 600 minutes.");

        RuleFor(x => x.Genre)
            .IsInEnum().WithMessage("Invalid movie genre.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid movie status.");

        RuleFor(x => x.TargetReach)
            .GreaterThanOrEqualTo(0).WithMessage("Target reach cannot be negative.");
    }
}