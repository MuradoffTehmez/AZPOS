using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MarketPOS.Infrastructure.Persistence.DesignTime;

/// <summary>
/// Design-time factory so `dotnet ef` can create the local context without
/// the UI host. The connection string here is used only for migration
/// scaffolding — runtime always reads appsettings.json.
/// </summary>
public sealed class LocalDbContextFactory : IDesignTimeDbContextFactory<LocalDbContext>
{
    /// <inheritdoc />
    public LocalDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LocalDbContext>()
            .UseSqlite("Data Source=marketpos-local.db")
            .Options;
        return new LocalDbContext(options);
    }
}
