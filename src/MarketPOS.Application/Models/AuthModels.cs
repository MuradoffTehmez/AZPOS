namespace MarketPOS.Application.Models;

/// <summary>
/// Signed-in employee identity carried by the user session; deliberately
/// excludes the password hash and other sensitive fields.
/// </summary>
/// <param name="Id">Employee id.</param>
/// <param name="FullName">Display name.</param>
/// <param name="Username">Login name.</param>
/// <param name="RoleName">RBAC role name (Cashier/Manager/Admin).</param>
public sealed record EmployeeInfo(int Id, string FullName, string Username, string RoleName);

/// <summary>
/// Outcome of a login attempt.
/// </summary>
public sealed record LoginResult
{
    /// <summary>Whether authentication succeeded.</summary>
    public bool Succeeded { get; private init; }

    /// <summary>User-facing failure message (Azerbaijani), or null on success.</summary>
    public string? FailureMessage { get; private init; }

    /// <summary>Authenticated employee, or null on failure.</summary>
    public EmployeeInfo? Employee { get; private init; }

    /// <summary>Creates a successful result.</summary>
    /// <param name="employee">Authenticated employee.</param>
    public static LoginResult Success(EmployeeInfo employee) => new() { Succeeded = true, Employee = employee };

    /// <summary>Creates a failed result with a user-facing message.</summary>
    /// <param name="message">Azerbaijani message shown on the login form.</param>
    public static LoginResult Failed(string message) => new() { Succeeded = false, FailureMessage = message };
}
