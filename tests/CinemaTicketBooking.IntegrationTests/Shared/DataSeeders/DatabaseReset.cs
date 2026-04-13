using CinemaTicketBooking.IntegrationTests.Shared.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;

public static class DatabaseReset
{
    public static async Task ResetAsync(string connectionString, CancellationToken ct = default)
    {
        await using var dbContext = TestDbContextFactory.Create(connectionString);

        const string truncateSql = """
            DO $$
            DECLARE
                statements text;
            BEGIN
                SELECT string_agg(
                    format('TRUNCATE TABLE %I.%I RESTART IDENTITY CASCADE;', schemaname, tablename),
                    ' '
                )
                INTO statements
                FROM pg_tables
                WHERE schemaname = 'public'
                  AND tablename <> '__EFMigrationsHistory';

                IF statements IS NOT NULL THEN
                    EXECUTE statements;
                END IF;
            END $$;
            """;

        await dbContext.Database.ExecuteSqlRawAsync(truncateSql, ct);
    }
}
