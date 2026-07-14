using MarketPOS.Application.Models;

namespace MarketPOS.Application.Abstractions;

/// <summary>
/// Inventory management: product catalog CRUD, stock levels and price labels.
/// </summary>
public interface IInventoryService
{
    /// <summary>Returns all products joined with stock and category for the inventory screen.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<ProductListItem>> GetProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns all category names for the edit form.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<string>> GetCategoryNamesAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates a product with its stock record; SKU and barcode must be unique.</summary>
    /// <param name="model">Product fields.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ProductEditResult> CreateProductAsync(ProductEditModel model, CancellationToken cancellationToken = default);

    /// <summary>Updates a product; price changes are audit-logged with old and new values.</summary>
    /// <param name="model">Product fields (Id must reference an existing product).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ProductEditResult> UpdateProductAsync(ProductEditModel model, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes a product (IsActive = false) so history stays intact.</summary>
    /// <param name="productId">Product id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ProductEditResult> DeactivateProductAsync(int productId, CancellationToken cancellationToken = default);
}
