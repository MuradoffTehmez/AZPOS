using MarketPOS.Application.Abstractions.Hardware;
using Microsoft.Extensions.Logging;

namespace MarketPOS.Infrastructure.Hardware;

/// <summary>
/// Development stand-in for an RS-232/USB scale: returns a random plausible
/// weight so the weight-based checkout flow can be exercised end to end.
/// </summary>
public sealed class MockScaleReader : IScaleReader
{
    private readonly ILogger<MockScaleReader> _logger;
    private readonly Random _random = new();

    /// <summary>Creates the mock.</summary>
    /// <param name="logger">Logger for simulated readings.</param>
    public MockScaleReader(ILogger<MockScaleReader> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<decimal> ReadWeightAsync(CancellationToken cancellationToken = default)
    {
        var weight = Math.Round((decimal)(_random.NextDouble() * 2.4 + 0.1), 3);
        _logger.LogInformation("Mock scale read: {Weight} kg", weight);
        return Task.FromResult(weight);
    }
}
