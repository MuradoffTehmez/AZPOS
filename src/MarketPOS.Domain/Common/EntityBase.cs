namespace MarketPOS.Domain.Common;

/// <summary>
/// Base class for all persisted entities; provides the surrogate primary key
/// that the generic repository layer relies on.
/// </summary>
public abstract class EntityBase
{
    /// <summary>Surrogate primary key.</summary>
    public int Id { get; set; }
}
