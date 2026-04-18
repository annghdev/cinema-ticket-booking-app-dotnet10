using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.WebServer.Middlewares;

/// <summary>
/// Catches unhandled exceptions in the HTTP pipeline, logs them once, and returns a problem response for API routes.
/// </summary>
public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Invokes the next middleware and handles failures.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (status, title, useErrorLog) = MapException(ex);
            if (useErrorLog)
            {
                logger.LogError(
                    ex,
                    "Unhandled exception for {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path.Value);
            }
            else
            {
                logger.LogWarning(
                    ex,
                    "Handled client exception for {Method} {Path}, status {Status}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    status);
            }

            if (context.Response.HasStarted)
                throw;

            if (context.Request.Path.StartsWithSegments("/api"))
            {
                await WriteProblemDetailsAsync(context, ex, status, title);
                return;
            }

            throw;
        }
    }

    private static (int Status, string Title, bool UseErrorLog) MapException(Exception ex)
    {
        switch (ex)
        {
            case ArgumentException:
                return (StatusCodes.Status400BadRequest, "Bad request.", false);

            case ValidationException:
                return (StatusCodes.Status400BadRequest, "Validation failed.", false);

            case InvalidOperationException ioe when ioe.Message.Contains("not found", StringComparison.OrdinalIgnoreCase):
                return (StatusCodes.Status404NotFound, "Not found.", false);

            case InvalidOperationException:
                return (StatusCodes.Status400BadRequest, "Bad request.", false);

            case DbUpdateConcurrencyException:
                return (StatusCodes.Status409Conflict, "The resource was modified by another request.", false);

            default:
                return (StatusCodes.Status500InternalServerError, "An error occurred while processing your request.", true);
        }
    }

    private async Task WriteProblemDetailsAsync(HttpContext context, Exception ex, int status, string title)
    {
        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var problem = new ProblemBody(
            Title: title,
            Status: status,
            Detail: environment.IsDevelopment() ? ex.ToString() : null,
            Errors: ex is ValidationException validationException
                ? validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(e => e.ErrorMessage).Distinct().ToArray())
                : null);

        await JsonSerializer.SerializeAsync(context.Response.Body, problem, JsonOptions, context.RequestAborted);
    }

    private record ProblemBody(
        string Title,
        int Status,
        string? Detail,
        Dictionary<string, string[]>? Errors);
}
