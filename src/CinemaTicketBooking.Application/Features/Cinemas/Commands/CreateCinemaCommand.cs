namespace CinemaTicketBooking.Application.Features;

public class CreateCinemaCommand : ICommand
{
    public string Name { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? Geo { get; set; }
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string CorrelationId {  get; set; } = string.Empty;
}

public class CreateCinemaHandler(IUnitOfWork uow)
{
    public async Task<Guid> Handle(CreateCinemaCommand cmd, CancellationToken ct)
    {
        var cinema = Cinema.Create(
            cmd.Name,
            cmd.ThumbnailUrl,
            cmd.Geo,
            cmd.Address,
            cmd.IsActive);

        uow.Cinemas.Add(cinema);
        await uow.CommitAsync(ct);
        return cinema.Id;
    }
}

public class CreateCinemaValidator : AbstractValidator<CreateCinemaCommand>
{
    public CreateCinemaValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Cinema name is required.")
            .MaximumLength(MaxLengthConsts.Name)
                .WithMessage($"Cinema name cannot exceed {MaxLengthConsts.Name} characters.");
                
        RuleFor(x => x.ThumbnailUrl)
            .NotEmpty().WithMessage("Thumbnail URL is required.")
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                .WithMessage("Thumbnail URL must be a valid absolute URL.")
            .MaximumLength(MaxLengthConsts.Url).WithMessage($"Thumbnail URL cannot exceed {MaxLengthConsts.Url} characters.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(MaxLengthConsts.Address).WithMessage($"Address cannot exceed {MaxLengthConsts.Address} characters.");
    }
}