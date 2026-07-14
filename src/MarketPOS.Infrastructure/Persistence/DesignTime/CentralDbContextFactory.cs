using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MarketPOS.Infrastructure.Persistence.DesignTime;

/// <summary>
/// Design-time factory so `dotnet ef` can create the central context without
/// the UI host. The connection string here is used only for migration
/// scaffolding — runtime always reads appsettings.json.
/// </summary>
public sealed class CentralDbContextFactory : IDesignTimeDbContextFactory<CentralDbContext>
{
    /// <inheritdoc />
    public CentralDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CentralDbContext>()
            .UseSqlServer("Server=.\\SQLEXPRESS;Database=MarketPOS;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;
        return new CentralDbContext(options);
    }
}
