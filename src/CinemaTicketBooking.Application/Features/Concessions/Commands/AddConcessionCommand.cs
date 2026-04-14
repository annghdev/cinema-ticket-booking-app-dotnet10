namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Adds a new concession item.
/// </summary>
public class AddConcessionCommand : ICommand
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles add-concession requests.
/// </summary>
public class AddConcessionHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Creates a concession item and persists changes.
    /// </summary>
    public async Task<Guid> Handle(AddConcessionCommand cmd, CancellationToken ct)
    {
        var concession = Concession.Create(
            name: cmd.Name,
            price: cmd.Price,
            imageUrl: cmd.ImageUrl,
            isAvailable: cmd.IsAvailable);

        uow.Concessions.Add(concession);
        await uow.CommitAsync(ct);
        return concession.Id;
    }
}

/// <summary>
/// Validates add-concession command payload.
/// </summary>
public class AddConcessionValidator : AbstractValidator<AddConcessionCommand>
{
    public AddConcessionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Concession name is required.")
            .MaximumLength(MaxLengthConsts.Name)
            .WithMessage($"Concession name cannot exceed {MaxLengthConsts.Name} characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("Image URL is required.")
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("Image URL must be a valid absolute URL.")
            .MaximumLength(MaxLengthConsts.Url)
            .WithMessage($"Image URL cannot exceed {MaxLengthConsts.Url} characters.");
    }
}
