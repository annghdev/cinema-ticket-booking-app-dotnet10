using CinemaTicketBooking.Domain;
using CinemaTicketBooking.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class BaseRepository<T> : IRepository<T>
    where T : class
{
    private readonly AppDbContext _db;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(AppDbContext db)
    {
        _db = db;
        _dbSet = db.Set<T>();
    }

    public virtual void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    public void Update(T entity)
    {
        _db.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return _dbSet.AnyAsync(predicate, ct);
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _dbSet.FindAsync(id, ct).AsTask();
    }

    public Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
    {
        return _dbSet.ToListAsync(ct).ContinueWith(t => t.Result.AsEnumerable(), ct);
    }

    public IQueryable<T> GetQueryFilter()
    {
        return _dbSet.AsNoTracking();
    }
}
