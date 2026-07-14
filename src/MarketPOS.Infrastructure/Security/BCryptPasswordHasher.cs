using MarketPOS.Application.Abstractions;

namespace MarketPOS.Infrastructure.Security;

/// <summary>
/// BCrypt implementation of <see cref="IPasswordHasher"/>. Work factor 12 keeps
/// hashing around ~250ms on typical POS hardware — slow enough against brute
/// force, fast enough for cashier login.
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    /// <inheritdoc />
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    /// <inheritdoc />
    public bool Verify(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
