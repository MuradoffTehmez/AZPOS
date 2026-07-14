using Microsoft.Extensions.DependencyInjection;

namespace MarketPOS.Application;

/// <summary>
/// Application layer dependency injection registration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers application-layer services. Business services (SaleService,
    /// InventoryService, ...) are added here in later implementation steps.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
