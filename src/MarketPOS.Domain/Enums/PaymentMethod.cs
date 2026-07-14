namespace MarketPOS.Domain.Enums;

/// <summary>
/// Payment method of a sale. Split (multi-tender) payments are recorded as
/// <see cref="Split"/>; per-tender breakdown arrives with the payment module.
/// </summary>
public enum PaymentMethod
{
    /// <summary>Cash payment.</summary>
    Cash = 0,

    /// <summary>Bank card via the external POS terminal.</summary>
    Card = 1,

    /// <summary>Combination of multiple tenders on one receipt.</summary>
    Split = 2
}
