using FluentResults;

namespace Mango.SharedKernel;

public interface IRepository<T> where T : BaseEntity
{
    Task<Result<T?>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<T>> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
