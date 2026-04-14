using CinemaTicketBooking.Domain;
using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Infrastructure.Cache;
using CinemaTicketBooking.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CinemaTicketBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("redis");
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "CinemaTicketBooking:";
        });
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            if (string.IsNullOrWhiteSpace(redisConnectionString))
            {
                throw new InvalidOperationException("Redis connection string 'redis' is not configured.");
            }

            return ConnectionMultiplexer.Connect(redisConnectionString);
        });
        services.AddScoped(typeof(ICacheService<>), typeof(RedisCacheService<>));

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
