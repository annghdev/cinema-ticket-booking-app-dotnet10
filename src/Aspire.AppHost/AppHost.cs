var builder = DistributedApplication.CreateBuilder(args);

var postgresUser = builder.AddParameter(
    "postgres-user",
    "postgres",
    publishValueAsDefault: true,
    secret: false);
var postgresPassword = builder.AddParameter(
    "postgres-password",
    "postgres",
    publishValueAsDefault: false,
    secret: true);

var postgres = builder.AddPostgres(
        "postgres",
        userName: postgresUser,
        password: postgresPassword)
    .WithPgWeb(pg => pg.WithHostPort(5050));

var cinemadb = postgres.AddDatabase("cinemadb");

var redis = builder.AddRedis("redis");

builder.AddProject<Projects.CinemaTicketBooking_WebServer>("cinematicketbooking-webserver")
    .WithReference(cinemadb)
    .WithReference(redis)
    .WaitFor(cinemadb)
    .WaitFor(redis);

builder.Build().Run();
