using MarketPOS.Domain.Common;

namespace MarketPOS.Domain.Entities;

/// <summary>
/// Receipt line. Snapshot pattern: the product's name and unit price are frozen
/// here at the moment of sale so historical reports stay correct when the
/// catalog changes later.
/// </summary>
public class SaleItem : EntityBase
{
    /// <summary>Owning sale.</summary>
    public int SaleId { get; set; }

    /// <summary>Sale navigation.</summary>
    public Sale? Sale { get; set; }

    /// <summary>Catalog product reference (for analytics; display data comes from snapshots).</summary>
    public int ProductId { get; set; }

    /// <summary>Product navigation.</summary>
    public Product? Product { get; set; }

    /// <summary>Product name frozen at the moment of sale.</summary>
    public required string ProductNameSnapshot { get; set; }

    /// <summary>Unit price frozen at the moment of sale.</summary>
    public decimal UnitPriceSnapshot { get; set; }

    /// <summary>Quantity sold; decimal to support weight-based products (e.g. 0.750 kg).</summary>
    public decimal Quantity { get; set; }

    /// <summary>Line total after line-level discount: UnitPriceSnapshot × Quantity − discount.</summary>
    public decimal LineTotal { get; set; }
}
