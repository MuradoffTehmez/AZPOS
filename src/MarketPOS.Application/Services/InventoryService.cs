using System.Globalization;
using MarketPOS.Application.Abstractions;
using MarketPOS.Application.Models;
using MarketPOS.Domain.Entities;
using MarketPOS.Domain.Enums;

namespace MarketPOS.Application.Services;

/// <summary>
/// Product catalog CRUD with stock records. Price changes are audit-logged
/// with old/new values; deletion is always soft (IsActive=false) so historical
/// sales keep their references.
/// </summary>
public sealed class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserSession _session;

    /// <summary>Creates the service.</summary>
    /// <param name="unitOfWork">Unit of work over the local store.</param>
    /// <param name="session">Signed-in user for audit records.</param>
    public InventoryService(IUnitOfWork unitOfWork, IUserSession session)
    {
        _unitOfWork = unitOfWork;
        _session = session;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductListItem>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Repository<Product>().GetAllAsync(cancellationToken).ConfigureAwait(false);
        var inventories = (await _unitOfWork.Repository<Inventory>().GetAllAsync(cancellationToken).ConfigureAwait(false))
            .ToDictionary(i => i.ProductId);
        var categories = (await _unitOfWork.Repository<Category>().GetAllAsync(cancellationToken).ConfigureAwait(false))
            .ToDictionary(c => c.Id, c => c.Name);

        return products
            .OrderBy(p => p.Name)
            .Select(p => new ProductListItem(
                p.Id, p.SKU, p.Barcode, p.Name,
                categories.GetValueOrDefault(p.CategoryId, string.Empty),
                p.UnitType == UnitType.Weight,
                p.Price, p.CostPrice, p.TaxRate, p.IsActive,
                inventories.TryGetValue(p.Id, out var inv) ? inv.QuantityOnHand : 0m,
                inventories.TryGetValue(p.Id, out var inv2) ? inv2.ReorderLevel : 0m))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetCategoryNamesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync(cancellationToken).ConfigureAwait(false);
        return categories.Select(c => c.Name).OrderBy(n => n).ToList();
    }

    /// <inheritdoc />
    public async Task<ProductEditResult> CreateProductAsync(ProductEditModel model, CancellationToken cancellationToken = default)
    {
        var validation = Validate(model);
        if (validation is not null)
        {
            return ProductEditResult.Failed(validation);
        }

        var products = _unitOfWork.Repository<Product>();
        if (await products.FirstOrDefaultAsync(p => p.SKU == model.SKU, cancellationToken).ConfigureAwait(false) is not null)
        {
            return ProductEditResult.Failed($"Bu SKU artıq mövcuddur: {model.SKU}");
        }

        if (await products.FirstOrDefaultAsync(p => p.Barcode == model.Barcode, cancellationToken).ConfigureAwait(false) is not null)
        {
            return ProductEditResult.Failed($"Bu barkod artıq mövcuddur: {model.Barcode}");
        }

        var category = await ResolveCategoryAsync(model.CategoryName, cancellationToken).ConfigureAwait(false);

        var product = new Product
        {
            SKU = model.SKU.Trim(),
            Barcode = model.Barcode.Trim(),
            Name = model.Name.Trim(),
            Category = category,
            UnitType = model.IsWeightBased ? UnitType.Weight : UnitType.Piece,
            Price = model.Price,
            CostPrice = model.CostPrice,
            TaxRate = model.TaxRate,
            IsActive = model.IsActive,
            Inventory = new Inventory
            {
                QuantityOnHand = model.QuantityOnHand,
                ReorderLevel = model.ReorderLevel
            }
        };

        await products.AddAsync(product, cancellationToken).ConfigureAwait(false);
        await WriteAuditAsync("ProductCreated", null, $"{product.SKU}:{product.Name}", cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return ProductEditResult.Success(product.Id);
    }

    /// <inheritdoc />
    public async Task<ProductEditResult> UpdateProductAsync(ProductEditModel model, CancellationToken cancellationToken = default)
    {
        var validation = Validate(model);
        if (validation is not null)
        {
            return ProductEditResult.Failed(validation);
        }

        var products = _unitOfWork.Repository<Product>();
        var product = await products.GetByIdAsync(model.Id, cancellationToken).ConfigureAwait(false);
        if (product is null)
        {
            return ProductEditResult.Failed("Məhsul tapılmadı.");
        }

        var skuOwner = await products.FirstOrDefaultAsync(
            p => p.SKU == model.SKU && p.Id != model.Id, cancellationToken).ConfigureAwait(false);
        if (skuOwner is not null)
        {
            return ProductEditResult.Failed($"Bu SKU artıq mövcuddur: {model.SKU}");
        }

        var barcodeOwner = await products.FirstOrDefaultAsync(
            p => p.Barcode == model.Barcode && p.Id != model.Id, cancellationToken).ConfigureAwait(false);
        if (barcodeOwner is not null)
        {
            return ProductEditResult.Failed($"Bu barkod artıq mövcuddur: {model.Barcode}");
        }

        if (product.Price != model.Price)
        {
            // Audit values are machine-readable history — always culture-invariant.
            await WriteAuditAsync("PriceChanged",
                product.Price.ToString("0.00", CultureInfo.InvariantCulture),
                model.Price.ToString("0.00", CultureInfo.InvariantCulture),
                cancellationToken, product.Id).ConfigureAwait(false);
        }

        product.SKU = model.SKU.Trim();
        product.Barcode = model.Barcode.Trim();
        product.Name = model.Name.Trim();
        product.Category = await ResolveCategoryAsync(model.CategoryName, cancellationToken).ConfigureAwait(false);
        product.UnitType = model.IsWeightBased ? UnitType.Weight : UnitType.Piece;
        product.Price = model.Price;
        product.CostPrice = model.CostPrice;
        product.TaxRate = model.TaxRate;
        product.IsActive = model.IsActive;
        products.Update(product);

        var inventories = _unitOfWork.Repository<Inventory>();
        var inventory = await inventories.FirstOrDefaultAsync(i => i.ProductId == product.Id, cancellationToken)
            .ConfigureAwait(false);
        if (inventory is null)
        {
            await inventories.AddAsync(new Inventory
            {
                ProductId = product.Id,
                QuantityOnHand = model.QuantityOnHand,
                ReorderLevel = model.ReorderLevel
            }, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            inventory.QuantityOnHand = model.QuantityOnHand;
            inventory.ReorderLevel = model.ReorderLevel;
            inventories.Update(inventory);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ProductEditResult.Success(product.Id);
    }

    /// <inheritdoc />
    public async Task<ProductEditResult> DeactivateProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        var products = _unitOfWork.Repository<Product>();
        var product = await products.GetByIdAsync(productId, cancellationToken).ConfigureAwait(false);
        if (product is null)
        {
            return ProductEditResult.Failed("Məhsul tapılmadı.");
        }

        product.IsActive = false;
        products.Update(product);
        await WriteAuditAsync("ProductDeactivated", product.SKU, null, cancellationToken, product.Id).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return ProductEditResult.Success(product.Id);
    }

    private static string? Validate(ProductEditModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return "Məhsul adı boş ola bilməz.";
        }

        if (string.IsNullOrWhiteSpace(model.SKU))
        {
            return "SKU boş ola bilməz.";
        }

        if (string.IsNullOrWhiteSpace(model.Barcode))
        {
            return "Barkod boş ola bilməz.";
        }

        if (string.IsNullOrWhiteSpace(model.CategoryName))
        {
            return "Kateqoriya boş ola bilməz.";
        }

        if (model.Price < 0 || model.CostPrice < 0)
        {
            return "Qiymət mənfi ola bilməz.";
        }

        if (model.TaxRate is < 0 or >= 1)
        {
            return "ƏDV dərəcəsi 0 ilə 1 arasında olmalıdır (məs. 0.18).";
        }

        if (model.QuantityOnHand < 0 || model.ReorderLevel < 0)
        {
            return "Stok dəyərləri mənfi ola bilməz.";
        }

        return null;
    }

    private async Task<Category> ResolveCategoryAsync(string categoryName, CancellationToken cancellationToken)
    {
        var name = categoryName.Trim();
        var existing = await _unitOfWork.Repository<Category>()
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return existing;
        }

        var category = new Category { Name = name };
        await _unitOfWork.Repository<Category>().AddAsync(category, cancellationToken).ConfigureAwait(false);
        return category;
    }

    private async Task WriteAuditAsync(
        string action, string? oldValue, string? newValue, CancellationToken cancellationToken, int? entityId = null)
    {
        await _unitOfWork.Repository<AuditLog>().AddAsync(new AuditLog
        {
            EmployeeId = _session.Current?.Id,
            Action = action,
            EntityName = nameof(Product),
            EntityId = entityId,
            OldValue = oldValue,
            NewValue = newValue,
            Timestamp = DateTime.UtcNow
        }, cancellationToken).ConfigureAwait(false);
    }
}
