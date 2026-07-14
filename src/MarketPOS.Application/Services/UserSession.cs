using MarketPOS.Application.Abstractions;
using MarketPOS.Application.Models;

namespace MarketPOS.Application.Services;

/// <summary>
/// In-memory user session for a single POS terminal.
/// </summary>
public sealed class UserSession : IUserSession
{
    /// <inheritdoc />
    public EmployeeInfo? Current { get; private set; }

    /// <inheritdoc />
    public bool IsInRole(string roleName) =>
        Current is not null && string.Equals(Current.RoleName, roleName, StringComparison.Ordinal);

    /// <inheritdoc />
    public void SignIn(EmployeeInfo employee) => Current = employee;

    /// <inheritdoc />
    public void SignOut() => Current = null;
}
