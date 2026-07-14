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
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
