using System.Linq.Expressions;
using MarketPOS.Domain.Common;

namespace MarketPOS.Application.Abstractions;

/// <summary>
/// Generic data-access abstraction over a single entity set. All persistence
/// goes through this interface — upper layers never see a DbContext.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IRepository<T> where T : EntityBase
{
    /// <summary>Returns the entity with the given id, or null when not found.</summary>
    /// <param name="id">Primary key value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Returns all entities of the set.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns entities matching the given predicate.</summary>
    /// <param name="predicate">Filter expression translated by the underlying provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>Returns the first entity matching the predicate, or null.</summary>
    /// <param name="predicate">Filter expression translated by the underlying provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>Stages a new entity for insertion; persisted on <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    /// <param name="entity">Entity to insert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Stages an update of an existing entity.</summary>
    /// <param name="entity">Entity to update.</param>
    void Update(T entity);

    /// <summary>Stages removal of an existing entity.</summary>
    /// <param name="entity">Entity to remove.</param>
    void Remove(T entity);
}
