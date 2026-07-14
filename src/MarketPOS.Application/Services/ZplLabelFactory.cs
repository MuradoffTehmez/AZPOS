using System.Globalization;
using MarketPOS.Application.Models;

namespace MarketPOS.Application.Services;

/// <summary>
/// Builds ZPL documents for the Zebra label printer (factory per the PRD's
/// document-generation pattern). Pure string assembly — printable through any
/// <see cref="Abstractions.Hardware.ILabelPrinter"/> implementation.
/// </summary>
public static class ZplLabelFactory
{
    /// <summary>
    /// Builds a 60x40mm price label: product name, price and a Code 128 barcode.
    /// </summary>
    /// <param name="product">Product to label.</param>
    /// <returns>Complete ZPL document.</returns>
    public static string BuildPriceLabel(ProductListItem product)
    {
        var price = product.Price.ToString("0.00", CultureInfo.InvariantCulture);
        var unitSuffix = product.IsWeightBased ? " AZN/kq" : " AZN";

        // ^CI28 selects UTF-8 so Azerbaijani product names print correctly.
        return $"""
            ^XA
            ^CI28
            ^PW480
            ^LL320
            ^FO20,20^A0N,35,35^FB440,2,0,L^FD{product.Name}^FS
            ^FO20,100^A0N,55,55^FD{price}{unitSuffix}^FS
            ^FO20,180^BY2^BCN,90,Y,N,N^FD{product.Barcode}^FS
            ^XZ
            """;
    }
}
