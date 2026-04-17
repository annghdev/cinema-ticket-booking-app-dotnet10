using CinemaTicketBooking.Application;
using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Domain;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CinemaTicketBooking.WebServer.ApiEndpoints;

/// <summary>
/// Public showtime APIs for browsing and state transitions.
/// </summary>
public static class ShowTimeEndpoints
{
    /// <summary>
    /// Maps showtime routes.
    /// </summary>
    public static void MapShowTimeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/showtimes")
            .WithTags("ShowTimes")
            .AllowAnonymous();

        group.MapGet("/", GetShowTimesAsync);
        group.MapGet("/paged", GetPagedShowTimesAsync);
        group.MapGet("/dropdown", GetShowTimeDropdownAsync);
        group.MapGet("/{id:guid}", GetShowTimeByIdAsync);

        group.MapPost("/", AddShowTimeAsync);
        group.MapPost("/{id:guid}/start", StartShowTimeAsync);
        group.MapPost("/{id:guid}/complete", CompleteShowTimeAsync);
        group.MapPost("/{id:guid}/cancel", CancelShowTimeAsync);
    }

    private static async Task<IResult> GetShowTimesAsync(
        [AsParameters] GetShowTimesRequest request,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<IReadOnlyList<ShowTimeDto>>(
            new GetShowTimesQuery
            {
                CinemaId = request.CinemaId,
                MovieId = request.MovieId,
                ScreenId = request.ScreenId,
                Status = request.Status,
                Date = request.Date,
                CorrelationId = string.Empty
            },
            ct);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetPagedShowTimesAsync(
        [AsParameters] GetPagedShowTimesRequest request,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<PagedResult<ShowTimeDto>>(
            new GetPagedShowTimesQuery
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                CinemaId = request.CinemaId,
                MovieId = request.MovieId,
                ScreenId = request.ScreenId,
                Status = request.Status,
                Date = request.Date,
                SortBy = request.SortBy,
                SortDirection = request.SortDirection,
                CorrelationId = string.Empty
            },
            ct);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetShowTimeDropdownAsync(
        [AsParameters] GetShowTimeDropdownRequest request,
        IMessageBus bus,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<IReadOnlyList<ShowTimeDropdownDto>>(
            new GetShowTimeDropdownQuery
            {
                CinemaId = request.CinemaId,
                MovieId = request.MovieId,
                ScreenId = request.ScreenId,
                Status = request.Status,
                MaxItems = request.MaxItems,
                CorrelationId = string.Empty
            },
            ct);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetShowTimeByIdAsync(Guid id, IMessageBus bus, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<ShowTimeDetailDto?>(
            new GetShowTimeByIdQuery
            {
                Id = id,
                CorrelationId = string.Empty
            },
            ct);

        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> AddShowTimeAsync(
        [FromBody] AddShowTimeCommand command,
        IMessageBus bus,
        CancellationToken ct)
    {
        try
        {
            var id = await bus.InvokeAsync<Guid>(command, ct);
            return Results.Ok(new { Id = id });
        }
        catch (InvalidOperationException ex)
        {
            return HandleInvalidOperation(ex);
        }
    }

    private static async Task<IResult> StartShowTimeAsync(Guid id, IMessageBus bus, CancellationToken ct)
    {
        try
        {
            await bus.InvokeAsync(new StartShowTimeCommand { Id = id, CorrelationId = string.Empty }, ct);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return HandleInvalidOperation(ex);
        }
    }

    private static async Task<IResult> CompleteShowTimeAsync(Guid id, IMessageBus bus, CancellationToken ct)
    {
        try
        {
            await bus.InvokeAsync(new CompleteShowTimeCommand { Id = id, CorrelationId = string.Empty }, ct);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return HandleInvalidOperation(ex);
        }
    }

    private static async Task<IResult> CancelShowTimeAsync(Guid id, IMessageBus bus, CancellationToken ct)
    {
        try
        {
            await bus.InvokeAsync(new CancelShowTimeCommand { Id = id, CorrelationId = string.Empty }, ct);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return HandleInvalidOperation(ex);
        }
    }

    private static IResult HandleInvalidOperation(InvalidOperationException ex)
    {
        if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return Results.NotFound(new { Message = ex.Message });
        }

        return Results.BadRequest(new { Message = ex.Message });
    }
}

public sealed class GetShowTimesRequest
{
    public Guid? CinemaId { get; init; }
    public Guid? MovieId { get; init; }
    public Guid? ScreenId { get; init; }
    public ShowTimeStatus? Status { get; init; }
    public DateOnly? Date { get; init; }
}

public sealed class GetPagedShowTimesRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public Guid? CinemaId { get; init; }
    public Guid? MovieId { get; init; }
    public Guid? ScreenId { get; init; }
    public ShowTimeStatus? Status { get; init; }
    public DateOnly? Date { get; init; }
    public string SortBy { get; init; } = "startat";
    public string SortDirection { get; init; } = "asc";
}

public sealed class GetShowTimeDropdownRequest
{
    public Guid? CinemaId { get; init; }
    public Guid? MovieId { get; init; }
    public Guid? ScreenId { get; init; }
    public ShowTimeStatus? Status { get; init; }
    public int MaxItems { get; init; } = 100;
}
