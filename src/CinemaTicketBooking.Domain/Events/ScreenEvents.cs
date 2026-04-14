namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a screen is created inside a cinema.
/// </summary>
public record ScreenCreated(
    Guid ScreenId,
    Guid CinemaId,
    string ScreenCode,
    ScreenType ScreenType,
    int RowOfSeats,
    int ColumnOfSeats,
    int TotalSeats,
    bool IsActive) : BaseDomainEvent;

/// <summary>
/// Raised when basic screen metadata is updated.
/// </summary>
public record ScreenBasicInfoUpdated(
    Guid ScreenId,
    Guid CinemaId,
    string ScreenCode,
    ScreenType ScreenType,
    int RowOfSeats,
    int ColumnOfSeats,
    int TotalSeats) : BaseDomainEvent;

/// <summary>
/// Raised when a screen is activated.
/// </summary>
public record ScreenActivated(Guid ScreenId, Guid CinemaId, string ScreenCode) : BaseDomainEvent;

/// <summary>
/// Raised when a screen is deactivated.
/// </summary>
public record ScreenDeactivated(Guid ScreenId, Guid CinemaId, string ScreenCode) : BaseDomainEvent;

/// <summary>
/// Raised when a seat inside a screen is activated.
/// </summary>
public record ScreenSeatActivated(
    Guid ScreenId,
    Guid SeatId,
    string ScreenCode,
    string SeatCode) : BaseDomainEvent;

/// <summary>
/// Raised when a seat inside a screen is deactivated.
/// </summary>
public record ScreenSeatDeactivated(
    Guid ScreenId,
    Guid SeatId,
    string ScreenCode,
    string SeatCode) : BaseDomainEvent;

/// <summary>
/// Raised when seats are generated for a Screen from its SeatMap.
/// Side effects: admin notification, cache updates.
/// </summary>
public record ScreenSeatsGenerated(
    Guid ScreenId,
    Guid CinemaId,
    string ScreenCode,
    ScreenType ScreenType,
    int TotalSeatsGenerated) : BaseDomainEvent;
