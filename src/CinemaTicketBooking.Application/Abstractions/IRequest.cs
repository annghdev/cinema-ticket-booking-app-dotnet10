namespace CinemaTicketBooking.Application;

public interface IRequest
{
    string CorrelationId { get; }
}
public interface ICachable
{
    string CacheKey { get; }
    TimeSpan? SlidingExpiration { get; }
}

public interface ICommand : IRequest;
public interface IQuery : IRequest;
public interface ICachableQuery : IQuery, ICachable;
