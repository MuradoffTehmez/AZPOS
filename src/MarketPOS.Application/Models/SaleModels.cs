using MarketPOS.Domain.Enums;

namespace MarketPOS.Application.Models;

/// <summary>
/// Product data the checkout screen needs; deliberately flat so the UI never
/// touches Domain entities.
/// </summary>
/// <param name="Id">Product id.</param>
/// <param name="Name">Display name.</param>
/// <param name="Barcode">Primary barcode.</param>
/// <param name="Price">Current retail price per unit.</param>
/// <param name="TaxRate">Tax rate as a fraction (0.18 = 18%).</param>
/// <param name="IsWeightBased">True when the product is sold by weight.</param>
/// <param name="StockOnHand">Current stock level.</param>
public sealed record ProductDto(
    int Id,
    string Name,
    string Barcode,
    decimal Price,
    decimal TaxRate,
    bool IsWeightBased,
    decimal StockOnHand);

/// <summary>One requested sale line.</summary>
/// <param name="ProductId">Product to sell.</param>
/// <param name="Quantity">Quantity (pieces or kg).</param>
public sealed record SaleLineRequest(int ProductId, decimal Quantity);

/// <summary>Checkout request to complete a sale.</summary>
/// <param name="Lines">Requested lines; must not be empty.</param>
/// <param name="PaymentMethod">Selected payment method.</param>
/// <param name="DiscountAmount">Receipt-level manual discount (0 when none).</param>
public sealed record CreateSaleRequest(
    IReadOnlyList<SaleLineRequest> Lines,
    PaymentMethod PaymentMethod,
    decimal DiscountAmount = 0m);

/// <summary>Outcome of a checkout attempt.</summary>
public sealed record SaleResult
{
    /// <summary>Whether the sale was persisted.</summary>
    public bool Succeeded { get; private init; }

    /// <summary>User-facing failure message (Azerbaijani), or null on success.</summary>
    public string? FailureMessage { get; private init; }

    /// <summary>Printable receipt, or null on failure.</summary>
    public ReceiptDocument? Receipt { get; private init; }

    /// <summary>Creates a successful result.</summary>
    /// <param name="receipt">Receipt of the persisted sale.</param>
    public static SaleResult Success(ReceiptDocument receipt) => new() { Succeeded = true, Receipt = receipt };

    /// <summary>Creates a failed result.</summary>
    /// <param name="message">Azerbaijani message shown at checkout.</param>
    public static SaleResult Failed(string message) => new() { Succeeded = false, FailureMessage = message };
}
