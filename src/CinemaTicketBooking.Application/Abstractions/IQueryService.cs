namespace CinemaTicketBooking.Application.Abstractions;

/// <summary>
/// Generic query service for executing raw SQL queries, typically using Dapper.
/// </summary>
public interface IQueryService
{
    /// <summary>
    /// Executes a query and returns a list of results.
    /// </summary>
    Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken ct = default);

    /// <summary>
    /// Executes a query and returns a single result or default.
    /// </summary>
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CancellationToken ct = default);

    /// <summary>
    /// Executes a query that returns multiple result sets.
    /// </summary>
    Task<T> QueryMultipleAsync<T>(string sql, object? param, Func<IMultipleResultReader, Task<T>> map, CancellationToken ct = default);
}

/// <summary>
/// Interface for reading multiple result sets from a single query execution.
/// </summary>
public interface IMultipleResultReader : IDisposable
{
    Task<T> ReadFirstAsync<T>();
    Task<T?> ReadFirstOrDefaultAsync<T>();
    Task<IReadOnlyList<T>> ReadAsync<T>();
}
