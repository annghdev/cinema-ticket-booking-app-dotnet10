using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.Application.Features;

/// <summary>
/// Gets a movie by id.
/// </summary>
public class GetMovieByIdQuery : IQuery
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Handles query for a specific movie.
/// </summary>
public class GetMovieByIdHandler(IUnitOfWork uow)
{
    /// <summary>
    /// Returns movie data when found; otherwise null.
    /// </summary>
    public async Task<MovieDto?> Handle(GetMovieByIdQuery query, CancellationToken ct)
    {
        var movie = await uow.Movies
            .GetQueryFilter()
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(x => new MovieDto(
                x.Id,
                x.Name,
                x.Description,
                x.ThumbnailUrl,
                x.Studio,
                x.Director,
                x.OfficialTrailerUrl,
                x.Duration,
                x.Genre,
                x.Status,
                x.CreatedAt))
            .FirstOrDefaultAsync(ct);

        return movie;
    }
}

/// <summary>
/// Validates query payload.
/// </summary>
public class GetMovieByIdValidator : AbstractValidator<GetMovieByIdQuery>
{
    public GetMovieByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Movie ID is required.");
    }
}
