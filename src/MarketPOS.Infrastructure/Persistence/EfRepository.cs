using System.Linq.Expressions;
using MarketPOS.Application.Abstractions;
using MarketPOS.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace MarketPOS.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IRepository{T}"/> over the context
/// owned by the enclosing unit of work.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public sealed class EfRepository<T> : IRepository<T> where T : EntityBase
{
    private readonly DbSet<T> _set;

    /// <summary>Creates the repository bound to the given context.</summary>
    /// <param name="context">Context shared with the owning unit of work.</param>
    public EfRepository(MarketPosDbContextBase context)
    {
        _set = context.Set<T>();
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _set.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _set.ToListAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        await _set.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        await _set.FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await _set.AddAsync(entity, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public void Update(T entity) => _set.Update(entity);

    /// <inheritdoc />
    public void Remove(T entity) => _set.Remove(entity);
}
