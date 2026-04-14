namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Updates basic information for an existing concession item.
/// </summary>
public class UpdateConcessionInfoCommand : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles concession info update requests.
/// </summary>
public class UpdateConcessionInfoHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Updates basic information for an existing concession and persists changes.
    /// </summary>
    public async Task Handle(UpdateConcessionInfoCommand cmd, CancellationToken ct)
    {
        var concession = await uow.Concessions.GetByIdAsync(cmd.Id, ct);
        if (concession is null)
        {
            throw new InvalidOperationException($"Concession with ID '{cmd.Id}' not found.");
        }

        concession.UpdateBasicInfo(
            name: cmd.Name,
            price: cmd.Price,
            imageUrl: cmd.ImageUrl);

        uow.Concessions.Update(concession);
        await uow.CommitAsync(ct);
    }
}

/// <summary>
/// Validates concession info update command payload.
/// </summary>
public class UpdateConcessionInfoValidator : AbstractValidator<UpdateConcessionInfoCommand>
{
    public UpdateConcessionInfoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Concession ID is required.");

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
