using System.Globalization;
using System.Text;
using MarketPOS.Application.Abstractions.Hardware;
using MarketPOS.Application.Models;
using Microsoft.Extensions.Logging;

namespace MarketPOS.Infrastructure.Hardware;

/// <summary>
/// Development stand-in for an ESC/POS thermal printer: renders the receipt as
/// fixed-width text and writes it to a file under receipts/ plus the log, so
/// output can be inspected without hardware.
/// </summary>
public sealed class MockReceiptPrinter : IReceiptPrinter
{
    private const int Width = 42;

    private readonly ILogger<MockReceiptPrinter> _logger;
    private readonly string _outputDirectory;

    /// <summary>Creates the mock.</summary>
    /// <param name="logger">Logger the rendered receipt is echoed to.</param>
    public MockReceiptPrinter(ILogger<MockReceiptPrinter> logger)
    {
        _logger = logger;
        _outputDirectory = Path.Combine(AppContext.BaseDirectory, "receipts");
    }

    /// <inheritdoc />
    public async Task PrintReceiptAsync(ReceiptDocument receipt, CancellationToken cancellationToken = default)
    {
        var text = Render(receipt);

        Directory.CreateDirectory(_outputDirectory);
        var fileName = $"receipt-{receipt.SaleId}-{receipt.SaleDate:yyyyMMdd-HHmmss}.txt";
        await File.WriteAllTextAsync(Path.Combine(_outputDirectory, fileName), text, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Mock receipt printed to {FileName}:\n{Receipt}", fileName, text);
    }

    private static string Render(ReceiptDocument receipt)
    {
        var culture = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();

        sb.AppendLine(Center(receipt.StoreName));
        sb.AppendLine(Center(receipt.SaleDate.ToString("dd.MM.yyyy HH:mm", culture)));
        sb.AppendLine($"Kassir: {receipt.CashierName}");
        sb.AppendLine($"Çek №: {receipt.SaleId}");
        sb.AppendLine(new string('-', Width));

        foreach (var line in receipt.Lines)
        {
            sb.AppendLine(line.Name);
            var amounts = string.Format(culture, "  {0:0.###} x {1:0.00}", line.Quantity, line.UnitPrice);
            var total = line.LineTotal.ToString("0.00", culture);
            sb.AppendLine(amounts.PadRight(Width - total.Length) + total);
        }

        sb.AppendLine(new string('-', Width));
        if (receipt.DiscountAmount > 0)
        {
            sb.AppendLine(FormatTotal("Endirim:", -receipt.DiscountAmount, culture));
        }

        sb.AppendLine(FormatTotal("CƏMİ:", receipt.TotalAmount, culture));
        sb.AppendLine(FormatTotal("O cümlədən ƏDV:", receipt.TaxAmount, culture));
        sb.AppendLine($"Ödəniş: {receipt.PaymentMethodDisplay}");
        sb.AppendLine();
        sb.AppendLine(Center("Bizi seçdiyiniz üçün təşəkkürlər!"));

        return sb.ToString();
    }

    private static string FormatTotal(string label, decimal amount, CultureInfo culture)
    {
        var value = amount.ToString("0.00", culture) + " ₼";
        return label.PadRight(Width - value.Length) + value;
    }

    private static string Center(string text) =>
        text.Length >= Width ? text : text.PadLeft((Width + text.Length) / 2).PadRight(Width);
}
