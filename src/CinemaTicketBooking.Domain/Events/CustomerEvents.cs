namespace CinemaTicketBooking.Domain;

/// <summary>
/// Raised when a customer profile is created.
/// </summary>
public record CustomerCreated(
    Guid CustomerId,
    string Name,
    string Email,
    string PhoneNumber,
    bool IsRegistered) : BaseDomainEvent;

/// <summary>
/// Raised when a guest customer registers an account (IsRegistered becomes true).
/// Side effects: send welcome email, migrate guest booking history, loyalty enrollment.
/// </summary>
public record CustomerRegistered(
    Guid CustomerId,
    string Name,
    string Email,
    string PhoneNumber) : BaseDomainEvent;
