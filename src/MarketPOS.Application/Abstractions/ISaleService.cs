using MarketPOS.Application.Models;

namespace MarketPOS.Application.Abstractions;

/// <summary>
/// Checkout operations: product lookup by barcode and sale completion.
/// </summary>
public interface ISaleService
{
    /// <summary>Finds an active product by barcode, or null when unknown.</summary>
    /// <param name="barcode">Scanned or typed barcode.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ProductDto?> GetProductByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a sale atomically: validates stock, freezes name/price
    /// snapshots, decreases inventory and persists to the local store with
    /// IsSyncedToServer=false (offline-first — never blocked by the network).
    /// </summary>
    /// <param name="request">Checkout request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<SaleResult> CreateSaleAsync(CreateSaleRequest request, CancellationToken cancellationToken = default);
}
