using MarketPOS.Domain.Common;

namespace MarketPOS.Domain.Entities;

/// <summary>
/// RBAC role. Phase 1 ships with Cashier, Manager and Admin.
/// </summary>
public class Role : EntityBase
{
    /// <summary>Role name of the cashier role.</summary>
    public const string Cashier = "Cashier";

    /// <summary>Role name of the store manager role.</summary>
    public const string Manager = "Manager";

    /// <summary>Role name of the administrator role.</summary>
    public const string Admin = "Admin";

    /// <summary>Unique role name (one of the constants above in Phase 1).</summary>
    public required string Name { get; set; }

    /// <summary>Employees assigned to this role.</summary>
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
