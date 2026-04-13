using CinemaTicketBooking.Application;
using CinemaTicketBooking.Domain;
using CinemaTicketBooking.Infrastructure.Persistence;
using JasperFx;
using JasperFx.Core;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.FluentValidation;
using Wolverine.Postgresql;

namespace CinemaTicketBooking.WebServer;

public static class WebApplicationBuilderExtensions
{
    public static void AddWolverine(this WebApplicationBuilder builder)
    {
        builder.Host.UseWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(typeof(IRequest).Assembly);

            var dbConectionString = builder.Configuration.GetConnectionString("cinemadb")!;
            opts.PersistMessagesWithPostgresql(dbConectionString);

            opts.Services.AddDbContextWithWolverineIntegration<AppDbContext>(
                x => x.UseNpgsql(dbConectionString));

            opts.UseFluentValidation();

            opts.Policies.AutoApplyTransactions();
            opts.Policies.UseDurableLocalQueues();

            opts.Policies
                .OnException<DbUpdateConcurrencyException>()
                .RetryWithCooldown(50.Milliseconds(), 250.Milliseconds(), 1.Seconds())
                .Then.MoveToErrorQueue();

            opts.Policies
            .OnException<ConcurrencyException>()
            .RetryWithCooldown(50.Milliseconds(), 250.Milliseconds(), 1.Seconds())
            .Then.MoveToErrorQueue();

            opts.AutoBuildMessageStorageOnStartup = AutoCreate.CreateOrUpdate;

            //opts.PublishAllMessages().Locally().MaximumParallelMessages(4).UseDurableInbox();

            opts.Publish(rule =>
            {
                rule.MessagesImplementing<IDomainEvent>();

                rule.ToLocalQueue("domain_events")
                    .MaximumParallelMessages(4)
                    .UseDurableInbox();
            });
        });
    }
}
