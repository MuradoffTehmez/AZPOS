using MarketPOS.Application.Models;
using MarketPOS.Application.Services;
using MarketPOS.Domain.Entities;
using MarketPOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MarketPOS.Application.Tests.Services;

/// <summary>
/// Inventory CRUD over a real SQLite database: uniqueness rules, price-change
/// audit and soft deletion.
/// </summary>
public sealed class InventoryServiceTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"marketpos-inv-{Guid.NewGuid():N}.db");
    private readonly UserSession _session = new();

    private LocalDbContext CreateContext() => new(
        new DbContextOptionsBuilder<LocalDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options);

    private InventoryService CreateService(LocalDbContext context) =>
        new(new UnitOfWork(context), _session);

    public InventoryServiceTests()
    {
        using var context = CreateContext();
        context.Database.Migrate();
        _session.SignIn(new EmployeeInfo(1, "Test Menecer", "menecer1", Role.Manager));
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    private static ProductEditModel NewProduct(string sku = "TST001", string barcode = "1000000000001") => new()
    {
        SKU = sku, Barcode = barcode, Name = "Test məhsul", CategoryName = "Test",
        Price = 5.00m, CostPrice = 3.00m, TaxRate = 0.18m,
        QuantityOnHand = 20m, ReorderLevel = 5m
    };

    [Fact]
    public async Task CreateProduct_PersistsProductInventoryAndCategory()
    {
        await using var context = CreateContext();
        var result = await CreateService(context).CreateProductAsync(NewProduct());

        Assert.True(result.Succeeded, result.FailureMessage);

        await using var verify = CreateContext();
        var product = await verify.Products.Include(p => p.Inventory).SingleAsync();
        Assert.Equal("TST001", product.SKU);
        Assert.Equal(20m, product.Inventory!.QuantityOnHand);
        Assert.Single(verify.Categories.Where(c => c.Name == "Test"));
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_Fails()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.CreateProductAsync(NewProduct());

        var result = await service.CreateProductAsync(NewProduct(barcode: "1000000000002"));

        Assert.False(result.Succeeded);
        Assert.Contains("SKU", result.FailureMessage);
    }

    [Fact]
    public async Task UpdateProduct_PriceChange_WritesAuditWithOldAndNewValues()
    {
        int productId;
        await using (var context = CreateContext())
        {
            var created = await CreateService(context).CreateProductAsync(NewProduct());
            productId = created.ProductId;
        }

        await using (var context = CreateContext())
        {
            var model = NewProduct() with { Id = productId, Price = 7.50m };
            var result = await CreateService(context).UpdateProductAsync(model);
            Assert.True(result.Succeeded, result.FailureMessage);
        }

        await using (var verify = CreateContext())
        {
            var audit = await verify.AuditLogs.SingleAsync(a => a.Action == "PriceChanged");
            Assert.Equal("5.00", audit.OldValue);
            Assert.Equal("7.50", audit.NewValue);
            Assert.Equal(productId, audit.EntityId);

            var product = await verify.Products.SingleAsync(p => p.Id == productId);
            Assert.Equal(7.50m, product.Price);
        }
    }

    [Fact]
    public async Task DeactivateProduct_SoftDeletes()
    {
        int productId;
        await using (var context = CreateContext())
        {
            productId = (await CreateService(context).CreateProductAsync(NewProduct())).ProductId;
        }

        await using (var context = CreateContext())
        {
            var result = await CreateService(context).DeactivateProductAsync(productId);
            Assert.True(result.Succeeded);
        }

        await using (var verify = CreateContext())
        {
            var product = await verify.Products.SingleAsync(p => p.Id == productId);
            Assert.False(product.IsActive);
        }
    }

    [Fact]
    public async Task CreateProduct_WithNegativePrice_Fails()
    {
        await using var context = CreateContext();
        var result = await CreateService(context).CreateProductAsync(NewProduct() with { Price = -1m });

        Assert.False(result.Succeeded);
    }
}
