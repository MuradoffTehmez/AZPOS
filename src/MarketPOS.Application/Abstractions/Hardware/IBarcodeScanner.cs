namespace MarketPOS.Application.Abstractions.Hardware;

/// <summary>
/// Barcode scanner abstraction. USB HID scanners type into the focused input,
/// so the checkout screen also accepts keyboard entry; this interface covers
/// scanners driven programmatically (serial/Bluetooth) and test simulation.
/// </summary>
public interface IBarcodeScanner
{
    /// <summary>Raised when a barcode is scanned.</summary>
    event EventHandler<string>? BarcodeScanned;
}
