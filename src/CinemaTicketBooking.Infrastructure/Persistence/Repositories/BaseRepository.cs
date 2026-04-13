using CinemaTicketBooking.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CinemaTicketBooking.Infrastructure.Persistence;

public class BaseRepository<T>(AppDbContext db) : IRepository<T>
    where T : class
{
    protected readonly DbSet<T> _dbSet = db.Set<T>();

    public virtual void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    public void Update(T entity)
    {
        db.Entry(entity).State = EntityState.Modified;
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
