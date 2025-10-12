using System.Linq.Expressions;
using Shared.Core.Helpers;

namespace Domain.Repositories;

public interface IRepository<T> where T : class
{
    Task<Result<T>> GetByIdAsync(int id);
    Task<Result<IEnumerable<T>>> GetAllAsync();
    Task<Result<IEnumerable<T>>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<Result<T>> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    void Add(T entity);
    void AddRange(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task<Result> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<Result> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<Result> RemoveAsync(T entity, CancellationToken cancellationToken = default);
    Task<Result> RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task<Result<bool>> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<Result<int>> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}