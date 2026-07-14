using MarketPOS.Domain.Common;

namespace MarketPOS.Domain.Entities;

/// <summary>
/// System user (cashier, manager, admin). Passwords are stored only as
/// BCrypt hashes — never plain text.
/// </summary>
public class Employee : EntityBase
{
    /// <summary>Full display name.</summary>
    public required string FullName { get; set; }

    /// <summary>Unique login name.</summary>
    public required string Username { get; set; }

    /// <summary>BCrypt hash of the password.</summary>
    public required string PasswordHash { get; set; }

    /// <summary>Assigned RBAC role id.</summary>
    public int RoleId { get; set; }

    /// <summary>Role navigation.</summary>
    public Role? Role { get; set; }

    /// <summary>Deactivated employees cannot log in but remain in history.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Shifts opened by this employee.</summary>
    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}
