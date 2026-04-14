namespace CinemaTicketBooking.Domain;

/// <summary>
/// Customer represents a person who books tickets.
/// Can be either a registered user (IsRegistered = true) or a guest identified by SessionId.
/// </summary>
public class Customer : AggregateRoot
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
    // Factory and data mutation
    // =============================================================

    /// <summary>
    /// Creates a new customer and raises a creation event.
    /// </summary>
    public static Customer Create(
        string name,
        string sessionId,
        string phoneNumber,
        string email,
        bool isRegistered)
    {
        var customer = new Customer
        {
            Name = name,
            SessionId = sessionId,
            PhoneNumber = phoneNumber,
            Email = email,
            IsRegistered = isRegistered
        };

        customer.RaiseEvent(new CustomerCreated(
            CustomerId: customer.Id,
            Name: customer.Name,
            Email: customer.Email,
            PhoneNumber: customer.PhoneNumber,
            IsRegistered: customer.IsRegistered));

        return customer;
    }

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
