using MarketPOS.Application.Abstractions.Hardware;
using Microsoft.Extensions.Logging;

namespace MarketPOS.Infrastructure.Hardware;

/// <summary>
/// Development stand-in for a Zebra label printer: writes the ZPL document to
/// a file under labels/ so the generated ZPL can be inspected (or sent to a
/// ZPL viewer) without hardware.
/// </summary>
public sealed class MockLabelPrinter : ILabelPrinter
{
    private readonly ILogger<MockLabelPrinter> _logger;
    private readonly string _outputDirectory;

    /// <summary>Creates the mock.</summary>
    /// <param name="logger">Logger the ZPL is echoed to.</param>
    public MockLabelPrinter(ILogger<MockLabelPrinter> logger)
    {
        _logger = logger;
        _outputDirectory = Path.Combine(AppContext.BaseDirectory, "labels");
    }

    /// <inheritdoc />
    public async Task PrintLabelAsync(string zpl, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_outputDirectory);
        var fileName = $"label-{DateTime.Now:yyyyMMdd-HHmmss-fff}.zpl";
        await File.WriteAllTextAsync(Path.Combine(_outputDirectory, fileName), zpl, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Mock ZPL label written to {FileName}:\n{Zpl}", fileName, zpl);
    }
}
