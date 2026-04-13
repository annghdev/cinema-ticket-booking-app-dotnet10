using CinemaTicketBooking.Domain;

namespace CinemaTicketBooking.Application.Abstractions;

public interface IUnitOfWork
{
    ICinemaRepository Cinemas { get; }
    IMovieRepository Movies { get; }
    IBookingRepository Bookings { get; }
    ITicketRepository Tickets { get; }
    IShowTimeRepository ShowTimes { get; }
    IScreenRepository Screens { get; }
    IConcessionRepository Concessions { get; }
    ICustomerRepository Customers { get; }
    IPricingPolicyRepository PricingPolicies { get; }

    Task CommitAsync(CancellationToken cancellationToken = default);
}
