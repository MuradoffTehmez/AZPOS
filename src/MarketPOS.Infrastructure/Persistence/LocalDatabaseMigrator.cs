using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarketPOS.Infrastructure.Persistence;

/// <summary>
/// Applies pending migrations to the local SQLite cache at startup so a fresh
/// terminal is usable immediately, with no manual DB setup. The central SQL
/// Server is migrated separately — it may be unreachable and must never block
/// startup (offline-first).
/// </summary>
internal sealed class LocalDatabaseMigrator : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LocalDatabaseMigrator> _logger;

    public LocalDatabaseMigrator(IServiceProvider serviceProvider, ILogger<LocalDatabaseMigrator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Local SQLite cache is up to date");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
