using CinemaTicketBooking.Domain;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace CinemaTicketBooking.Application.Features.Tests;

public class TestRequest : IRequest
{
    public string Message { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = Guid.Empty.ToString();
}

public class TestRequestHandler(IMessageBus bus, IRepository<Movie> movieRepository, ILogger<TestRequestHandler> logger)
{
    public async Task Handle(TestRequest request, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Handling TestRequest with Text: {Text} and CorrelationId: {CorrelationId}",
            request.Message,
            request.CorrelationId);

        var movie = new Movie
        {
            Name = request.Message,
            Genre = MovieGenre.Action,
            Duration = 120,
        };
        movieRepository.Add(movie);
    }
}

public class TestRequestValidator : AbstractValidator<TestRequest>
{
    public TestRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty().WithMessage("Message must not be empty.");
        RuleFor(x => x.CorrelationId).NotEmpty().WithMessage("CorrelationId must not be empty.");
    }
}
