using MarketPOS.Application.Abstractions;
using MarketPOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarketPOS.Infrastructure.Persistence;

/// <summary>
/// Seeds the local store with the three Phase 1 roles and a default admin
/// account on first run, so a fresh terminal is immediately usable.
/// Runs after <see cref="LocalDatabaseMigrator"/> (hosted services start in
/// registration order).
/// </summary>
internal sealed class DataSeeder : IHostedService
{
    private const string DefaultAdminUsername = "admin";
    private const string DefaultAdminPassword = "Admin123!";

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(IServiceProvider serviceProvider, ILogger<DataSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (!await context.Roles.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            context.Roles.AddRange(
                new Role { Name = Role.Cashier },
                new Role { Name = Role.Manager },
                new Role { Name = Role.Admin });
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Seeded default roles (Cashier, Manager, Admin)");
        }

        if (!await context.Employees.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var adminRole = await context.Roles.SingleAsync(r => r.Name == Role.Admin, cancellationToken)
                .ConfigureAwait(false);

            context.Employees.Add(new Employee
            {
                FullName = "Sistem İnzibatçısı",
                Username = DefaultAdminUsername,
                PasswordHash = passwordHasher.Hash(DefaultAdminPassword),
                RoleId = adminRole.Id,
                IsActive = true
            });
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // Password is intentionally logged once: first-run bootstrap on a fresh
            // terminal; the admin must change it immediately.
            _logger.LogWarning(
                "Default admin user created (username: {Username}, password: {Password}). Change the password immediately",
                DefaultAdminUsername, DefaultAdminPassword);
        }

        await SeedDemoProductsAsync(context, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Seeds a handful of demo products on first run so checkout can be tried
    /// immediately; skipped as soon as any real product exists.
    /// </summary>
    private async Task SeedDemoProductsAsync(LocalDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Products.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var category = new Category { Name = "Ərzaq" };
        context.Categories.Add(category);

        var products = new[]
        {
            new Product
            {
                SKU = "BRD001", Barcode = "4760000000017", Name = "Çörək",
                Category = category, UnitType = Domain.Enums.UnitType.Piece,
                Price = 0.60m, CostPrice = 0.40m, TaxRate = 0m, IsActive = true
            },
            new Product
            {
                SKU = "MLK001", Barcode = "4760000000024", Name = "Süd 1L",
                Category = category, UnitType = Domain.Enums.UnitType.Piece,
                Price = 2.50m, CostPrice = 1.80m, TaxRate = 0.18m, IsActive = true
            },
            new Product
            {
                SKU = "APL001", Barcode = "2000000000012", Name = "Alma (kq)",
                Category = category, UnitType = Domain.Enums.UnitType.Weight,
                Price = 3.20m, CostPrice = 2.10m, TaxRate = 0.18m, IsActive = true
            }
        };
        context.Products.AddRange(products);

        context.Inventories.AddRange(
            new Inventory { Product = products[0], QuantityOnHand = 100m, ReorderLevel = 20m },
            new Inventory { Product = products[1], QuantityOnHand = 50m, ReorderLevel = 10m },
            new Inventory { Product = products[2], QuantityOnHand = 25.5m, ReorderLevel = 5m });

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Seeded {Count} demo products", products.Length);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
