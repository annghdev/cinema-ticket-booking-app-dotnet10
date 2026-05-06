using CinemaTicketBooking.Application.Abstractions;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class QueryService(AppDbContext db) : IQueryService
{
    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        var connection = db.Database.GetDbConnection();
        var results = await connection.QueryAsync<T>(new CommandDefinition(sql, param, cancellationToken: ct));
        return results.ToList();
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        var connection = db.Database.GetDbConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(new CommandDefinition(sql, param, cancellationToken: ct));
    }

    public async Task<T> QueryMultipleAsync<T>(string sql, object? param, Func<IMultipleResultReader, Task<T>> map, CancellationToken ct = default)
    {
        var connection = db.Database.GetDbConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(sql, param, cancellationToken: ct));
        var reader = new DapperMultipleResultReader(multi);
        return await map(reader);
    }

    private class DapperMultipleResultReader(SqlMapper.GridReader reader) : IMultipleResultReader
    {
        public async Task<T> ReadFirstAsync<T>() => await reader.ReadFirstAsync<T>();
        public async Task<T?> ReadFirstOrDefaultAsync<T>() => await reader.ReadFirstOrDefaultAsync<T>();
        public async Task<IReadOnlyList<T>> ReadAsync<T>()
        {
            var results = await reader.ReadAsync<T>();
            return results.ToList();
        }

        public void Dispose() => reader.Dispose();
    }
}
