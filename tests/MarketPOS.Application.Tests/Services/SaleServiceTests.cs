using System.Linq.Expressions;
using MarketPOS.Application.Abstractions;
using MarketPOS.Application.Models;
using MarketPOS.Application.Services;
using MarketPOS.Domain.Entities;
using MarketPOS.Domain.Enums;
using Moq;

namespace MarketPOS.Application.Tests.Services;

/// <summary>
/// Core checkout scenarios: successful sale, insufficient-stock rejection and
/// snapshot correctness when the catalog price changes after the sale.
/// </summary>
public class SaleServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRepository<Product>> _products = new();
    private readonly Mock<IRepository<Inventory>> _inventories = new();
    private readonly Mock<IRepository<Shift>> _shifts = new();
    private readonly Mock<IRepository<Sale>> _sales = new();
    private readonly Mock<IRepository<AuditLog>> _auditLogs = new();
    private readonly UserSession _session = new();
    private readonly SaleService _service;

    private readonly Product _product;
    private readonly Inventory _inventory;
    private Sale? _persistedSale;

    public SaleServiceTests()
    {
        _unitOfWork.Setup(u => u.Repository<Product>()).Returns(_products.Object);
        _unitOfWork.Setup(u => u.Repository<Inventory>()).Returns(_inventories.Object);
        _unitOfWork.Setup(u => u.Repository<Shift>()).Returns(_shifts.Object);
        _unitOfWork.Setup(u => u.Repository<Sale>()).Returns(_sales.Object);
        _unitOfWork.Setup(u => u.Repository<AuditLog>()).Returns(_auditLogs.Object);

        _product = new Product
        {
            Id = 1, SKU = "MLK001", Barcode = "4760000000024", Name = "Süd 1L",
            CategoryId = 1, UnitType = UnitType.Piece,
            Price = 2.50m, CostPrice = 1.80m, TaxRate = 0.18m, IsActive = true
        };
        _inventory = new Inventory { Id = 1, ProductId = 1, QuantityOnHand = 10m, ReorderLevel = 2m };

        _products.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(_product);
        _inventories
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Inventory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_inventory);
        _shifts
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Shift, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Shift { Id = 5, EmployeeId = 7, OpenedAt = DateTime.Now, Status = ShiftStatus.Open });
        _sales
            .Setup(r => r.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .Callback<Sale, CancellationToken>((sale, _) => _persistedSale = sale)
            .Returns(Task.CompletedTask);

        _session.SignIn(new EmployeeInfo(7, "Test Kassir", "kassir1", Role.Cashier));
        _service = new SaleService(_unitOfWork.Object, _session);
    }

    private static CreateSaleRequest SingleLineRequest(decimal quantity = 2m) =>
        new(new[] { new SaleLineRequest(1, quantity) }, PaymentMethod.Cash);

    [Fact]
    public async Task CreateSale_WithSufficientStock_SucceedsAndDecreasesInventory()
    {
        var result = await _service.CreateSaleAsync(SingleLineRequest(quantity: 2m));

        Assert.True(result.Succeeded);
        Assert.Equal(8m, _inventory.QuantityOnHand);
        Assert.Equal(5.00m, result.Receipt!.TotalAmount);
        Assert.False(_persistedSale!.IsSyncedToServer);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSale_WithInsufficientStock_FailsWithoutPersisting()
    {
        var result = await _service.CreateSaleAsync(SingleLineRequest(quantity: 11m));

        Assert.False(result.Succeeded);
        Assert.Contains("stok", result.FailureMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(10m, _inventory.QuantityOnHand);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateSale_Snapshot_IsIndependentOfLaterPriceChange()
    {
        await _service.CreateSaleAsync(SingleLineRequest(quantity: 1m));

        var item = Assert.Single(_persistedSale!.Items);
        Assert.Equal(2.50m, item.UnitPriceSnapshot);
        Assert.Equal("Süd 1L", item.ProductNameSnapshot);

        // Catalog changes after the sale must not affect the frozen snapshot.
        _product.Price = 9.99m;
        _product.Name = "Yeni ad";

        Assert.Equal(2.50m, item.UnitPriceSnapshot);
        Assert.Equal("Süd 1L", item.ProductNameSnapshot);
    }

    [Fact]
    public async Task CreateSale_CalculatesTaxFromTaxInclusivePrice()
    {
        var result = await _service.CreateSaleAsync(SingleLineRequest(quantity: 2m));

        // 5.00 tax-inclusive at 18% → tax portion = 5.00 * 0.18 / 1.18 = 0.76.
        Assert.Equal(0.76m, result.Receipt!.TaxAmount);
    }

    [Fact]
    public async Task CreateSale_WithEmptyCart_Fails()
    {
        var result = await _service.CreateSaleAsync(
            new CreateSaleRequest(Array.Empty<SaleLineRequest>(), PaymentMethod.Cash));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task CreateSale_WithoutOpenShift_AutoOpensShift()
    {
        _shifts
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Shift, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shift?)null);

        var result = await _service.CreateSaleAsync(SingleLineRequest());

        Assert.True(result.Succeeded);
        _shifts.Verify(r => r.AddAsync(
            It.Is<Shift>(s => s.EmployeeId == 7 && s.Status == ShiftStatus.Open),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
