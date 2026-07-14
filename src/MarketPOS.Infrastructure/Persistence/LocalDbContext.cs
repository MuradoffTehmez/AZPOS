using Microsoft.EntityFrameworkCore;

namespace MarketPOS.Infrastructure.Persistence;

/// <summary>
/// Local SQLite cache — the offline-first operational store every terminal
/// writes to; the sync service pushes committed data to the central server.
/// </summary>
public sealed class LocalDbContext : MarketPosDbContextBase
{
    /// <summary>Creates the local context.</summary>
    /// <param name="options">SQLite provider options.</param>
    public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options)
    {
    }
}
