using CinemaTicketBooking.Domain.Abstractions;

namespace CinemaTicketBooking.Domain;

public interface IAuditableEntity : IDefaultEntity, ITrackable, ISoftDeletalbe
{
}
