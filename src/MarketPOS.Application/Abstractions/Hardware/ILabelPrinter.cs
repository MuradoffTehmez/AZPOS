namespace MarketPOS.Application.Abstractions.Hardware;

/// <summary>
/// Label printer abstraction (Zebra/ZPL in production, mock writing ZPL to a
/// file in development). Used by the inventory module for price labels.
/// </summary>
public interface ILabelPrinter
{
    /// <summary>Prints a label described by raw ZPL.</summary>
    /// <param name="zpl">ZPL document content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PrintLabelAsync(string zpl, CancellationToken cancellationToken = default);
}
