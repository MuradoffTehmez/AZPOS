using MarketPOS.Domain.Common;
using MarketPOS.Domain.Enums;

namespace MarketPOS.Domain.Entities;

/// <summary>
/// Sellable product in the catalog.
/// </summary>
public class Product : EntityBase
{
    /// <summary>Unique stock keeping unit code.</summary>
    public required string SKU { get; set; }

    /// <summary>Primary barcode (EAN/UPC); multi-barcode support arrives in Phase 2.</summary>
    public required string Barcode { get; set; }

    /// <summary>Display name shown at checkout and on receipts.</summary>
    public required string Name { get; set; }

    /// <summary>Category id.</summary>
    public int CategoryId { get; set; }

    /// <summary>Category navigation.</summary>
    public Category? Category { get; set; }

    /// <summary>Whether the product is sold per piece or by weight.</summary>
    public UnitType UnitType { get; set; }

    /// <summary>Current retail price per unit (per piece or per kg).</summary>
    public decimal Price { get; set; }

    /// <summary>Current purchase cost per unit, used for margin reporting.</summary>
    public decimal CostPrice { get; set; }

    /// <summary>Tax (ƏDV) rate as a fraction, e.g. 0.18 for 18%.</summary>
    public decimal TaxRate { get; set; }

    /// <summary>Inactive products are hidden from checkout but kept for history.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Stock record navigation (one-to-one).</summary>
    public Inventory? Inventory { get; set; }
}
