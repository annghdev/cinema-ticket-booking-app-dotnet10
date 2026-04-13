using CinemaTicketBooking.Domain;
using CinemaTicketBooking.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaTicketBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
        //services.AddScoped<ITicketRepository, TicketRepository>();
        return services;
    }
}
