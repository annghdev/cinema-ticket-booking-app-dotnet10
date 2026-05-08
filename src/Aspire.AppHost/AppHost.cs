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


var minioUser = builder.AddParameter(
    "minio-user",
    "admin",
    publishValueAsDefault: true,
    secret: false);
var minioPassword = builder.AddParameter(
    "minio-password",
    "admin123",
    publishValueAsDefault: false,
    secret: true);

var minio = builder.AddMinioContainer(
    "minio",
    rootUser: minioUser,
    rootPassword: minioPassword);

builder.AddProject<Projects.CinemaTicketBooking_WebServer>("cinematicketbooking-webserver")
    .WithReference(cinemadb)
    .WithReference(redis)
    .WithReference(minio)
    .WaitFor(cinemadb)
    .WaitFor(redis);

builder.Build().Run();
