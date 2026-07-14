namespace MarketPOS.Application.Abstractions;

/// <summary>
/// Password hashing abstraction. Implemented with BCrypt in Infrastructure;
/// plain-text passwords are never stored or logged.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Hashes a plain-text password with a per-password salt.</summary>
    /// <param name="password">Plain-text password.</param>
    /// <returns>Self-contained hash string safe to persist.</returns>
    string Hash(string password);

    /// <summary>Verifies a plain-text password against a stored hash.</summary>
    /// <param name="password">Plain-text password entered by the user.</param>
    /// <param name="passwordHash">Stored hash to verify against.</param>
    /// <returns>True when the password matches.</returns>
    bool Verify(string password, string passwordHash);
}
