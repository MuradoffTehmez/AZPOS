using MarketPOS.Application.Models;
using MarketPOS.Application.Services;
using MarketPOS.Domain.Entities;
using MarketPOS.Domain.Enums;
using MarketPOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MarketPOS.Application.Tests.Integration;

/// <summary>
/// End-to-end sale flow over a real SQLite database and the real migration:
/// login-equivalent session → checkout → persisted sale with frozen snapshots,
/// decreased stock and the offline-first sync flag.
/// </summary>
public sealed class SaleFlowIntegrationTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"marketpos-test-{Guid.NewGuid():N}.db");

    private LocalDbContext CreateContext() => new(
        new DbContextOptionsBuilder<LocalDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options);

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    [Fact]
    public async Task FullSaleFlow_PersistsSale_DecreasesStock_AndFreezesSnapshot()
    {
        int productId, employeeId, saleId;

        // Arrange: migrated DB with a role, an employee, one product in stock.
        await using (var context = CreateContext())
        {
            await context.Database.MigrateAsync();

            var role = new Role { Name = Role.Cashier };
            var employee = new Employee
            {
                FullName = "Test Kassir", Username = "kassir1",
                PasswordHash = "hash", Role = role, IsActive = true
            };
            var category = new Category { Name = "Ərzaq" };
            var product = new Product
            {
                SKU = "MLK001", Barcode = "4760000000024", Name = "Süd 1L",
                Category = category, UnitType = UnitType.Piece,
                Price = 2.50m, CostPrice = 1.80m, TaxRate = 0.18m, IsActive = true
            };
            context.AddRange(role, employee, category, product,
                new Inventory { Product = product, QuantityOnHand = 10m, ReorderLevel = 2m });
            await context.SaveChangesAsync();

            productId = product.Id;
            employeeId = employee.Id;
        }

        // Act: complete a checkout through the real service + unit of work.
        await using (var context = CreateContext())
        {
            var session = new UserSession();
            session.SignIn(new EmployeeInfo(employeeId, "Test Kassir", "kassir1", Role.Cashier));
            var saleService = new SaleService(new UnitOfWork(context), session);

            var result = await saleService.CreateSaleAsync(new CreateSaleRequest(
                new[] { new SaleLineRequest(productId, 2m) }, PaymentMethod.Cash));

            Assert.True(result.Succeeded, result.FailureMessage);
            Assert.Equal(5.00m, result.Receipt!.TotalAmount);
            saleId = result.Receipt.SaleId;
            Assert.True(saleId > 0);
        }

        // Assert on a fresh context: persisted state is correct.
        await using (var context = CreateContext())
        {
            var sale = await context.Sales.Include(s => s.Items).SingleAsync(s => s.Id == saleId);
            Assert.False(sale.IsSyncedToServer);
            Assert.Equal(SaleStatus.Completed, sale.Status);

            var item = Assert.Single(sale.Items);
            Assert.Equal("Süd 1L", item.ProductNameSnapshot);
            Assert.Equal(2.50m, item.UnitPriceSnapshot);

            var stock = await context.Inventories.SingleAsync(i => i.ProductId == productId);
            Assert.Equal(8m, stock.QuantityOnHand);

            // Snapshot must survive a later catalog price change.
            var product = await context.Products.SingleAsync(p => p.Id == productId);
            product.Price = 9.99m;
            product.Name = "Süd 1L (yeni qiymət)";
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var item = await context.SaleItems.SingleAsync(i => i.SaleId == saleId);
            Assert.Equal("Süd 1L", item.ProductNameSnapshot);
            Assert.Equal(2.50m, item.UnitPriceSnapshot);
        }
    }
}
