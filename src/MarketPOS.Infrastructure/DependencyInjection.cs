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
    /// Registers infrastructure services. DbContexts (SQL Server + SQLite),
    /// repositories, unit of work and hardware adapters are added here in step 3+.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">Application configuration providing connection strings.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}
