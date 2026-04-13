using CinemaTicketBooking.IntegrationTests.Shared.Fixtures;
using CinemaTicketBooking.Infrastructure.Persistence;

namespace CinemaTicketBooking.IntegrationTests.InfrastructureTests.PersistenceTests;

[Collection(DatabaseCollection.Name)]
public abstract class RepositoryTestBase(PostgresContainerFixture databaseFixture)
{
    protected PostgresContainerFixture DatabaseFixture { get; } = databaseFixture;

    protected AppDbContext CreateDbContext() => DatabaseFixture.CreateDbContext();
}
