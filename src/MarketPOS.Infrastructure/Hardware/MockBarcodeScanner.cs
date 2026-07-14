using MarketPOS.Application.Abstractions.Hardware;

namespace MarketPOS.Infrastructure.Hardware;

/// <summary>
/// Development stand-in for a programmatically driven scanner. Real USB HID
/// scanners type into the focused textbox and need no adapter at all.
/// </summary>
public sealed class MockBarcodeScanner : IBarcodeScanner
{
    /// <inheritdoc />
    public event EventHandler<string>? BarcodeScanned;

    /// <summary>Simulates a scan, raising <see cref="BarcodeScanned"/>.</summary>
    /// <param name="barcode">Barcode value to emit.</param>
    public void SimulateScan(string barcode) => BarcodeScanned?.Invoke(this, barcode);
}
