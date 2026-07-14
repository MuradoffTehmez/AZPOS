using MarketPOS.Application.Models;

namespace MarketPOS.Application.Abstractions;

/// <summary>
/// Authenticates employees against the local store and records audit entries.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Attempts to sign in with the given credentials. On success the user
    /// session is populated and an audit record is written.
    /// </summary>
    /// <param name="username">Login name.</param>
    /// <param name="password">Plain-text password (never persisted).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<LoginResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
}
