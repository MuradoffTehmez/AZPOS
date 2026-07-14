using MarketPOS.Application.Abstractions;
using MarketPOS.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarketPOS.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection registration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Connection string name of the central SQL Server database.</summary>
    public const string CentralConnectionName = "CentralDatabase";

    /// <summary>Connection string name of the local SQLite offline cache.</summary>
    public const string LocalCacheConnectionName = "LocalCache";

    /// <summary>
    /// Registers infrastructure services: both database contexts, the unit of
    /// work over the local store, and the startup migrator for the local cache.
    /// Hardware adapters are added in later steps.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">Application configuration providing connection strings.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CentralDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(CentralConnectionName)));

        services.AddDbContext<LocalDbContext>(options =>
            options.UseSqlite(ResolveLocalCachePath(configuration.GetConnectionString(LocalCacheConnectionName))));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHostedService<LocalDatabaseMigrator>();

        return services;
    }

    /// <summary>
    /// Anchors a relative SQLite file path to the application directory —
    /// otherwise it would resolve against the process working directory, which
    /// varies with how the app is launched.
    /// </summary>
    /// <param name="connectionString">Raw connection string from configuration.</param>
    /// <returns>Connection string with an absolute database path.</returns>
    private static string ResolveLocalCachePath(string? connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString ?? "Data Source=marketpos-local.db");
        if (!Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.Combine(AppContext.BaseDirectory, builder.DataSource);
        }

        return builder.ConnectionString;
    }
}
