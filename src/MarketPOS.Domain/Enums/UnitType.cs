namespace MarketPOS.Domain.Enums;

/// <summary>
/// How a product is measured and sold at checkout.
/// </summary>
public enum UnitType
{
    /// <summary>Sold per piece (count-based).</summary>
    Piece = 0,

    /// <summary>Sold by weight (scale/PLU based, e.g. kg).</summary>
    Weight = 1
}
