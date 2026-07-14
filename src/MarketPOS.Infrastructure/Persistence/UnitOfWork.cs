using MarketPOS.Application.Abstractions;
using MarketPOS.Domain.Common;

namespace MarketPOS.Infrastructure.Persistence;

/// <summary>
/// EF Core unit of work over the local offline-first store. The context is
/// owned and disposed by the DI scope, not by this class.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly LocalDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    /// <summary>Creates the unit of work over the local store.</summary>
    /// <param name="context">Scoped local database context.</param>
    public UnitOfWork(LocalDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public IRepository<T> Repository<T>() where T : EntityBase
    {
        if (_repositories.TryGetValue(typeof(T), out var existing))
        {
            return (IRepository<T>)existing;
        }

        var repository = new EfRepository<T>(_context);
        _repositories[typeof(T)] = repository;
        return repository;
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
