using CinemaTicketBooking.Domain;
using CinemaTicketBooking.Domain.Repositories;
using CinemaTicketBooking.Infrastructure.Persistence.Repositories;
using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Infrastructure.Cache;
using CinemaTicketBooking.Infrastructure.Payments.Momo;
using CinemaTicketBooking.Infrastructure.Payments;
using CinemaTicketBooking.Infrastructure.Payments.Vnpay;
using CinemaTicketBooking.Infrastructure.Persistence;
using CinemaTicketBooking.Infrastructure.QrCodes;
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
        services.Configure<TicketLockingOptions>(configuration.GetSection(TicketLockingOptions.SectionName));

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "CinemaTicketBooking:";
            });
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
                redisOptions.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(redisOptions);
            });
            services.AddScoped<ICacheService, RedisCacheService>();
            services.AddScoped<ITicketLocker, RedisTicketLocker>();
        }
        else
        {
            services.AddScoped<ITicketLocker, NoRedisTicketLocker>();
            services.AddScoped<ICacheService, NoOpCacheService>();
        }

        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ICinemaRepository, CinemaRepository>();
        services.AddScoped<IConcessionRepository, ConcessionRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IScreenRepository, ScreenRepository>();
        services.AddScoped<IPricingPolicyRepository, PricingPolicyRepository>();
        services.AddScoped<ISeatSelectionPolicyRepository, SeatSelectionPolicyRepository>();
        services.AddScoped<IShowTimeRepository, ShowTimeRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<ISlideRepository, SlideRepository>();
        services.AddScoped<IUnitOfWork, EFUnitOfWork>();
        services.AddScoped<DataSeeder>();
        services.AddHttpClient();

        // Payment services
        services.Configure<MomoOptions>(configuration.GetSection(MomoOptions.SectionName));
        services.Configure<VnpayOptions>(configuration.GetSection(VnpayOptions.SectionName));
        services.AddScoped<IPaymentService, NoPaymentGatewayService>();
        services.AddScoped<IPaymentService, MomoPaymentService>();
        services.AddScoped<IPaymentService, VnpayPaymentService>();
        services.AddScoped<IPaymentServiceFactory, PaymentServiceFactory>();
        services.AddSingleton<IPaymentRealtimePublisher, NoOpPaymentRealtimePublisher>();

        services.AddSingleton<IQrCodeGenerator, QrCodeGenerator>();

        return services;
    }
}
