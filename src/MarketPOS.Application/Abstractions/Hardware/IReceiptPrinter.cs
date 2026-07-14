using MarketPOS.Application.Models;

namespace MarketPOS.Application.Abstractions.Hardware;

/// <summary>
/// Receipt printer abstraction (ESC/POS thermal printer in production, mock in
/// development).
/// </summary>
public interface IReceiptPrinter
{
    /// <summary>Prints the given receipt.</summary>
    /// <param name="receipt">Receipt content to print.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PrintReceiptAsync(ReceiptDocument receipt, CancellationToken cancellationToken = default);
}
