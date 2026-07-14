using Microsoft.EntityFrameworkCore;

namespace MarketPOS.Infrastructure.Persistence;

/// <summary>
/// Central SQL Server database — the authoritative store that terminals sync into.
/// </summary>
public sealed class CentralDbContext : MarketPosDbContextBase
{
    /// <summary>Creates the central context.</summary>
    /// <param name="options">SQL Server provider options.</param>
    public CentralDbContext(DbContextOptions<CentralDbContext> options) : base(options)
    {
    }
}
