using CinemaTicketBooking.Application.Features.Tests;
using CinemaTicketBooking.Infrastructure;
using CinemaTicketBooking.Infrastructure.Persistence;
using CinemaTicketBooking.WebServer;
using JasperFx;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.AddWolverine();

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseRouting();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/apis", () => Results.Redirect("scalar/v1"));

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapPost("/test-requests", async (IMessageBus bus, TestRequest request) =>
{
    await bus.InvokeAsync(request);
    return Results.Ok();
});


using var scope = app.Services.CreateScope();
try
{
    // Migrate Orders
    var orderContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await orderContext.Database.MigrateAsync();

    // Seed data
    //var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    //await seeder.SeedAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Error during database migration or seeding: {ex.Message}");
    throw;
}

return await app.RunJasperFxCommands(args);
