using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarketPOS.Sync;

/// <summary>
/// Background service that pushes locally stored (offline) sales to the central
/// SQL Server. Stub for step 1 — the real sync queue arrives with the sales
/// module (step 5), so the UI is never blocked by network availability.
/// </summary>
public sealed class SyncBackgroundService : BackgroundService
{
    private readonly ILogger<SyncBackgroundService> _logger;

    /// <summary>
    /// Creates the sync service.
    /// </summary>
    /// <param name="logger">Logger for sync diagnostics.</param>
    public SyncBackgroundService(ILogger<SyncBackgroundService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sync background service started (stub — sync logic arrives in step 5)");
        return Task.CompletedTask;
    }
}
