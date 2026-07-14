using MarketPOS.Domain.Common;

namespace MarketPOS.Application.Abstractions;

/// <summary>
/// Groups repository operations into a single atomic commit. Backed by the
/// local offline-first store in Phase 1; the sync service pushes committed
/// data to the central server in the background.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Returns the repository for the given entity type (cached per unit of work).</summary>
    /// <typeparam name="T">Entity type.</typeparam>
    IRepository<T> Repository<T>() where T : EntityBase;

    /// <summary>Commits all staged changes atomically.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of affected rows.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
