using CinemaTicketBooking.Application;
using CinemaTicketBooking.IntegrationTests.Shared.DataSeeders;
using CinemaTicketBooking.IntegrationTests.Shared.Fixtures;
using CinemaTicketBooking.Infrastructure.Persistence;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CinemaTicketBooking.IntegrationTests.InfrastructureTests.PersistenceTests;

[Collection(DatabaseCollection.Name)]
public sealed class UnitOfWorkBehaviorTests(
    PostgresContainerFixture databaseFixture,
    WolverineFixture wolverineFixture) : IClassFixture<WolverineFixture>
{
    [Fact]
    public async Task CommitAsync_Should_ApplyAuditFields_OnAdd()
    {
        await databaseFixture.ResetDatabaseAsync();
        await using var db = databaseFixture.CreateDbContext();

        var movie = IntegrationEntityBuilder.Movie();
        db.Movies.Add(movie);

        var unitOfWork = CreateUnitOfWork(db, wolverineFixture.MessageBus, "integration-tester");
        await unitOfWork.CommitAsync();
        await db.SaveChangesAsync();

        var saved = await db.Movies.IgnoreQueryFilters().SingleAsync(x => x.Id == movie.Id);
        saved.CreatedBy.Should().Be("integration-tester");
        saved.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, 10.Seconds());
        saved.UpdatedBy.Should().BeNull();
        saved.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task CommitAsync_Should_ApplyAuditFields_OnUpdate()
    {
        await databaseFixture.ResetDatabaseAsync();
        await using var db = databaseFixture.CreateDbContext();

        var movie = IntegrationEntityBuilder.Movie(name: "Before");
        db.Movies.Add(movie);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var tracked = await db.Movies.SingleAsync(x => x.Id == movie.Id);
        tracked.Name = "After";

        var unitOfWork = CreateUnitOfWork(db, wolverineFixture.MessageBus, "integration-tester");
        await unitOfWork.CommitAsync();
        await db.SaveChangesAsync();

        var saved = await db.Movies.IgnoreQueryFilters().SingleAsync(x => x.Id == movie.Id);
        saved.CreatedAt.Should().NotBe(default);
        saved.CreatedBy.Should().BeNull();
        saved.UpdatedBy.Should().Be("integration-tester");
        saved.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CommitAsync_Should_ConvertDeleteToSoftDelete_AndRespectQueryFilter()
    {
        await databaseFixture.ResetDatabaseAsync();
        await using var db = databaseFixture.CreateDbContext();

        var movie = IntegrationEntityBuilder.Movie();
        db.Movies.Add(movie);
        await db.SaveChangesAsync();

        db.Movies.Remove(movie);
        var unitOfWork = CreateUnitOfWork(db, wolverineFixture.MessageBus, "integration-tester");
        await unitOfWork.CommitAsync();
        await db.SaveChangesAsync();

        var hiddenByFilter = await db.Movies.SingleOrDefaultAsync(x => x.Id == movie.Id);
        hiddenByFilter.Should().BeNull();

        var softDeleted = await db.Movies
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == movie.Id);
        softDeleted.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CommitAsync_Should_ClearDomainEvents_AfterDispatch()
    {
        await databaseFixture.ResetDatabaseAsync();
        await using var db = databaseFixture.CreateDbContext();
        wolverineFixture.Probe.Reset();

        var movie = IntegrationEntityBuilder.Movie(status: Domain.MovieStatus.Upcoming);
        db.Movies.Add(movie);

        movie.PromoteToNowShowing();
        movie.Events.Should().ContainSingle();

        var unitOfWork = CreateUnitOfWork(db, wolverineFixture.MessageBus, "integration-tester");
        await unitOfWork.CommitAsync();

        movie.Events.Should().BeEmpty();
    }

    [Fact]
    public async Task CommitAsync_Should_DispatchAndHandleDomainEvent()
    {
        await databaseFixture.ResetDatabaseAsync();
        await using var db = databaseFixture.CreateDbContext();
        wolverineFixture.Probe.Reset();

        var movie = IntegrationEntityBuilder.Movie(status: Domain.MovieStatus.Upcoming);
        db.Movies.Add(movie);

        movie.PromoteToNowShowing();
        var unitOfWork = CreateUnitOfWork(db, wolverineFixture.MessageBus, "integration-tester");

        await unitOfWork.CommitAsync();

        var handled = await wolverineFixture.Probe.WaitForHandledEventAsync(TimeSpan.FromSeconds(3));
        handled.Should().BeOfType<Domain.MoviePromotedToNowShowing>();
    }

    private static EFUnitOfWork CreateUnitOfWork(AppDbContext db, Wolverine.IMessageBus messageBus, string userName)
    {
        var userContext = new Mock<IUserContext>();
        userContext.Setup(x => x.UserName).Returns(userName);
        userContext.Setup(x => x.IsAuthenticated).Returns(true);
        userContext.Setup(x => x.UserId).Returns(Guid.CreateVersion7());
        userContext.Setup(x => x.CustomerId).Returns((Guid?)null);
        userContext.Setup(x => x.Permissions).Returns(new HashSet<string>());
        userContext.Setup(x => x.HasPermission(It.IsAny<string>())).Returns(false);

        return new EFUnitOfWork(
            db,
            messageBus,
            userContext.Object,
            new ServiceCollection().BuildServiceProvider());
    }
}
