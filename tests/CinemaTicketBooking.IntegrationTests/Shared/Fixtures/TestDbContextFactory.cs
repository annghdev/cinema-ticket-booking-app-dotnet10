using CinemaTicketBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketBooking.IntegrationTests.Shared.Fixtures;

public static class TestDbContextFactory
{
    public static AppDbContext Create(string connectionString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .Options;

        return new AppDbContext(options);
    }
}
