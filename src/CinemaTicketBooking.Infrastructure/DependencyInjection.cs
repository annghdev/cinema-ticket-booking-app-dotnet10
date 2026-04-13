using CinemaTicketBooking.Domain;
using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaTicketBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ICinemaRepository, CinemaRepository>();
        services.AddScoped<IConcessionRepository, ConcessionRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IScreenRepository, ScreenRepository>();
        services.AddScoped<IPricingPolicyRepository, PricingPolicyRepository>();
        services.AddScoped<IShowTimeRepository, ShowTimeRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUnitOfWork, EFUnitOfWork>();
        return services;
    }
}
