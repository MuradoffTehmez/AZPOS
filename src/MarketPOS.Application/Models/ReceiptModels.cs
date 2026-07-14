namespace MarketPOS.Application.Models;

/// <summary>
/// One printed line of a receipt.
/// </summary>
/// <param name="Name">Product name (snapshot at sale time).</param>
/// <param name="Quantity">Quantity sold (pieces or kg).</param>
/// <param name="UnitPrice">Unit price (snapshot at sale time).</param>
/// <param name="LineTotal">Line total.</param>
public sealed record ReceiptLine(string Name, decimal Quantity, decimal UnitPrice, decimal LineTotal);

/// <summary>
/// Printable receipt content, produced by the sale service and consumed by
/// <see cref="Abstractions.Hardware.IReceiptPrinter"/> implementations.
/// </summary>
public sealed record ReceiptDocument
{
    /// <summary>Persisted sale id the receipt belongs to.</summary>
    public required int SaleId { get; init; }

    /// <summary>Store display name.</summary>
    public required string StoreName { get; init; }

    /// <summary>Cashier display name.</summary>
    public required string CashierName { get; init; }

    /// <summary>Sale timestamp (local).</summary>
    public required DateTime SaleDate { get; init; }

    /// <summary>Receipt lines.</summary>
    public required IReadOnlyList<ReceiptLine> Lines { get; init; }

    /// <summary>Total paid.</summary>
    public required decimal TotalAmount { get; init; }

    /// <summary>Tax portion included in the total.</summary>
    public required decimal TaxAmount { get; init; }

    /// <summary>Receipt-level discount applied.</summary>
    public required decimal DiscountAmount { get; init; }

    /// <summary>Payment method display text (Azerbaijani).</summary>
    public required string PaymentMethodDisplay { get; init; }
}
