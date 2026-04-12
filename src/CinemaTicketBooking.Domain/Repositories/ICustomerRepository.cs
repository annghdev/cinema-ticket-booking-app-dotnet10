namespace CinemaTicketBooking.Domain;

/// <summary>
/// Repository interface for the Customer entity.
/// Manages both registered users and guest customers identified by SessionId.
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
}
