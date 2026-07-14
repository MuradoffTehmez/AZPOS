using MarketPOS.Domain.Common;

namespace MarketPOS.Domain.Entities;

/// <summary>
/// Stock level of a single product (single-branch scope for Phase 1).
/// Surrogate Id plus a unique ProductId keeps the generic repository usable.
/// </summary>
public class Inventory : EntityBase
{
    /// <summary>Product this stock record belongs to (unique).</summary>
    public int ProductId { get; set; }

    /// <summary>Product navigation.</summary>
    public Product? Product { get; set; }

    /// <summary>Quantity currently on hand; decimal to support weight-based units.</summary>
    public decimal QuantityOnHand { get; set; }

    /// <summary>Threshold below which a low-stock warning is raised.</summary>
    public decimal ReorderLevel { get; set; }
}
