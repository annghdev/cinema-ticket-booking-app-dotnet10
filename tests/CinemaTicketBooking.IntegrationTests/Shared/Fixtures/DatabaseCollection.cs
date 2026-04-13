namespace CinemaTicketBooking.IntegrationTests.Shared.Fixtures;

[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<PostgresContainerFixture>
{
    public const string Name = "postgres-database";
}
