using MarketPOS.Application.Models;
using MarketPOS.Application.Services;

namespace MarketPOS.Application.Tests.Services;

/// <summary>
/// The generated ZPL must be a complete document carrying the name, price and
/// barcode, with UTF-8 enabled for Azerbaijani characters.
/// </summary>
public class ZplLabelFactoryTests
{
    private static ProductListItem Product(bool weightBased = false) => new(
        1, "TST001", "4760000000024", "Kərə yağı", "Süd məhsulları",
        weightBased, 12.40m, 9.00m, 0.18m, true, 10m, 2m);

    [Fact]
    public void BuildPriceLabel_ContainsNamePriceAndBarcode()
    {
        var zpl = ZplLabelFactory.BuildPriceLabel(Product());

        Assert.StartsWith("^XA", zpl);
        Assert.EndsWith("^XZ", zpl);
        Assert.Contains("^CI28", zpl);
        Assert.Contains("Kərə yağı", zpl);
        Assert.Contains("12.40 AZN", zpl);
        Assert.Contains("^FD4760000000024^FS", zpl);
    }

    [Fact]
    public void BuildPriceLabel_WeightBased_UsesPerKgSuffix()
    {
        var zpl = ZplLabelFactory.BuildPriceLabel(Product(weightBased: true));

        Assert.Contains("12.40 AZN/kq", zpl);
    }
}
