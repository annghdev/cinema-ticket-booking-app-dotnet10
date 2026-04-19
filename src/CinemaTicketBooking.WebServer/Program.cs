using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Infrastructure;
using CinemaTicketBooking.Infrastructure.Auth;
using CinemaTicketBooking.Infrastructure.Persistence;
using CinemaTicketBooking.WebServer;
using CinemaTicketBooking.WebServer.ApiEndpoints;
using CinemaTicketBooking.WebServer.CronJobs;
using CinemaTicketBooking.WebServer.Hubs;
using CinemaTicketBooking.WebServer.Middlewares;
using JasperFx;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.AddWolverine();

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthInfrastructure(builder.Configuration);
builder.Services.AddSingleton<ITicketRealtimePublisher, SignalRTicketRealtimePublisher>();
builder.Services.AddHostedService<TicketLockRecoveryHostedService>();


// =======================================================
// ************* Rate Limiting Configuration *************
// =======================================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Policy "fixed" - 100 requests / 1 minute / 1 IP
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    // Policy "sliding" - 10 requests / 10s / 1 IP (for sensitive endpoints)
    options.AddSlidingWindowLimiter("sliding", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.SegmentsPerWindow = 2;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

    // Global limiter — apply default for all requests based on IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5
            }));
});

// =======================================================
// ****************** CORS Configuration *****************
// =======================================================
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    // Policy 1: Allow All for Development
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    // Policy 2: Allow Specific domains for Production
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});


var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseCors("AllowSpecificOrigins");
}
else
{
    app.UseCors("AllowAll");
}
//app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/apis", () => Results.Redirect("scalar/v1"));

app.MapAuthEndpoints();
app.MapBookingEndpoints();
app.MapShowTimeEndpoints();
app.MapCinemaEndpoints();
app.MapMovieEndpoints();
app.MapScreenEndpoints();
app.MapHub<TicketStatusHub>("/hubs/tickets").AllowAnonymous();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


await using var scope = app.Services.CreateAsyncScope();
try
{
    // Migrate Orders
    var orderContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await orderContext.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    await IdentityDataSeeder.SeedAsync(roleManager, loggerFactory.CreateLogger("IdentitySeed"));

    // Seed data
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Error during database migration or seeding: {ex.Message}");
    throw;
}

return await app.RunJasperFxCommands(args);

/// <summary>
/// Exposes the implicit program class to Alba / WebApplicationFactory integration tests.
/// </summary>
public partial class Program;
