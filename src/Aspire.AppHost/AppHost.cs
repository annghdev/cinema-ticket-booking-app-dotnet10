var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CinemaTicketBooking_WebServer>("cinematicketbooking-webserver");

builder.Build().Run();
