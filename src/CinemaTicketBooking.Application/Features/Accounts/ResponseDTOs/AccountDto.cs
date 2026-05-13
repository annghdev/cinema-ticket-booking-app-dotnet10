namespace CinemaTicketBooking.Application.Features;

public sealed record AccountDto(
    Guid Id,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    string? AvatarUrl,
    bool IsLockedOut,
    DateTimeOffset? LockoutEnd,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> Roles,
    Guid? CustomerId,
    string? CustomerName
);
