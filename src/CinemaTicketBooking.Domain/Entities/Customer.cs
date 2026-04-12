namespace CinemaTicketBooking.Domain;

/// <summary>
/// Customer represents a person who books tickets.
/// Can be either a registered user (IsRegistered = true) or a guest identified by SessionId.
/// </summary>
public class Customer : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Temporary session identifier for guest customers (not yet registered).
    /// Used to match ticket locks with the customer who initiated them.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsRegistered { get; set; }

    // =============================================================
    // State Transitions
    // =============================================================

    /// <summary>
    /// Marks a guest customer as registered after account creation.
    /// Side effects: send welcome email, migrate guest booking history, loyalty enrollment.
    /// </summary>
    public void Register()
    {
        if (IsRegistered)
            throw new InvalidOperationException("Customer is already registered.");

        IsRegistered = true;

        RaiseEvent(new CustomerRegistered(
            CustomerId: Id,
            Name: Name,
            Email: Email,
            PhoneNumber: PhoneNumber));
    }
}
