namespace MarketPOS.Application.Models;

/// <summary>
/// One row of the inventory screen: product joined with its stock and category.
/// </summary>
/// <param name="Id">Product id.</param>
/// <param name="SKU">Stock keeping unit code.</param>
/// <param name="Barcode">Primary barcode.</param>
/// <param name="Name">Display name.</param>
/// <param name="CategoryName">Category display name.</param>
/// <param name="IsWeightBased">True when sold by weight.</param>
/// <param name="Price">Retail price.</param>
/// <param name="CostPrice">Purchase cost.</param>
/// <param name="TaxRate">Tax rate as a fraction.</param>
/// <param name="IsActive">Whether the product is sellable.</param>
/// <param name="QuantityOnHand">Current stock.</param>
/// <param name="ReorderLevel">Low-stock warning threshold.</param>
public sealed record ProductListItem(
    int Id,
    string SKU,
    string Barcode,
    string Name,
    string CategoryName,
    bool IsWeightBased,
    decimal Price,
    decimal CostPrice,
    decimal TaxRate,
    bool IsActive,
    decimal QuantityOnHand,
    decimal ReorderLevel);

/// <summary>
/// Editable product fields for create/update. CategoryName is resolved to an
/// existing category or a new one is created with that name.
/// </summary>
public sealed record ProductEditModel
{
    /// <summary>Product id; 0 for a new product.</summary>
    public int Id { get; init; }

    /// <summary>Stock keeping unit code (unique).</summary>
    public required string SKU { get; init; }

    /// <summary>Primary barcode (unique).</summary>
    public required string Barcode { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>Category display name.</summary>
    public required string CategoryName { get; init; }

    /// <summary>True when sold by weight.</summary>
    public bool IsWeightBased { get; init; }

    /// <summary>Retail price.</summary>
    public decimal Price { get; init; }

    /// <summary>Purchase cost.</summary>
    public decimal CostPrice { get; init; }

    /// <summary>Tax rate as a fraction (0.18 = 18%).</summary>
    public decimal TaxRate { get; init; }

    /// <summary>Whether the product is sellable.</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>Stock on hand.</summary>
    public decimal QuantityOnHand { get; init; }

    /// <summary>Low-stock warning threshold.</summary>
    public decimal ReorderLevel { get; init; }
}

/// <summary>Outcome of a product create/update.</summary>
public sealed record ProductEditResult
{
    /// <summary>Whether the operation persisted.</summary>
    public bool Succeeded { get; private init; }

    /// <summary>User-facing failure message (Azerbaijani), or null on success.</summary>
    public string? FailureMessage { get; private init; }

    /// <summary>Persisted product id, or 0 on failure.</summary>
    public int ProductId { get; private init; }

    /// <summary>Creates a successful result.</summary>
    /// <param name="productId">Persisted product id.</param>
    public static ProductEditResult Success(int productId) => new() { Succeeded = true, ProductId = productId };

    /// <summary>Creates a failed result.</summary>
    /// <param name="message">Azerbaijani message shown on the edit form.</param>
    public static ProductEditResult Failed(string message) => new() { Succeeded = false, FailureMessage = message };
}
