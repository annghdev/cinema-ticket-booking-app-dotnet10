namespace CinemaTicketBooking.Application.Features;

public record SystemAccountDetailDto(
    Guid Id,
    string UserName,
    string Email,
    List<string> Roles,
    List<string> Permissions);
