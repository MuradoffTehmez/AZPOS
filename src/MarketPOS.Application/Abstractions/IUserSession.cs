using MarketPOS.Application.Models;

namespace MarketPOS.Application.Abstractions;

/// <summary>
/// Holds the currently signed-in employee for the lifetime of the process.
/// Registered as a singleton — one terminal, one active user at a time.
/// </summary>
public interface IUserSession
{
    /// <summary>Currently signed-in employee, or null before login.</summary>
    EmployeeInfo? Current { get; }

    /// <summary>Returns true when a user is signed in with the given role.</summary>
    /// <param name="roleName">Role name to check (e.g. Role.Admin).</param>
    bool IsInRole(string roleName);

    /// <summary>Stores the authenticated employee.</summary>
    /// <param name="employee">Authenticated employee identity.</param>
    void SignIn(EmployeeInfo employee);

    /// <summary>Clears the session (logout or shift end).</summary>
    void SignOut();
}
