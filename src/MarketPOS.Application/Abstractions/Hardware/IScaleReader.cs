namespace MarketPOS.Application.Abstractions.Hardware;

/// <summary>
/// Weighing scale abstraction (RS-232/USB in production, mock in development).
/// </summary>
public interface IScaleReader
{
    /// <summary>Reads the current stable weight in kilograms.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Weight in kg with 3 decimal places.</returns>
    Task<decimal> ReadWeightAsync(CancellationToken cancellationToken = default);
}
