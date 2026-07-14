namespace MarketPOS.Domain.Enums;

/// <summary>
/// Lifecycle state of a sale transaction.
/// </summary>
public enum SaleStatus
{
    /// <summary>Sale completed and paid.</summary>
    Completed = 0,

    /// <summary>Sale cancelled before payment was finalized.</summary>
    Cancelled = 1,

    /// <summary>Sale fully or partially returned with reference to the original receipt.</summary>
    Returned = 2
}
