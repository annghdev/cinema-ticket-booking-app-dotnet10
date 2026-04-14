namespace CinemaTicketBooking.Application;

public sealed class PagedResult<T>(IEnumerable<T> items, int totalItems, int pageNumber, int pageSize)
{
    public IEnumerable<T> Items { get; init; } = items;
    public int PageNumber { get; init; } = pageNumber;
    public int PageSize { get; init; } = pageSize;
    public int TotalItems { get; init; } = totalItems;
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}