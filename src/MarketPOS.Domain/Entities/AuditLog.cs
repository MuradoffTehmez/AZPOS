using MarketPOS.Domain.Common;

namespace MarketPOS.Domain.Entities;

/// <summary>
/// Append-only audit record of a critical operation (price change, discount,
/// deletion, login). Rows are never updated or deleted by any role.
/// </summary>
public class AuditLog : EntityBase
{
    /// <summary>Acting employee, or null for system-initiated operations.</summary>
    public int? EmployeeId { get; set; }

    /// <summary>Employee navigation.</summary>
    public Employee? Employee { get; set; }

    /// <summary>Operation performed, e.g. "PriceChanged", "Login".</summary>
    public required string Action { get; set; }

    /// <summary>Entity type the operation touched, e.g. "Product".</summary>
    public required string EntityName { get; set; }

    /// <summary>Primary key of the touched entity, or null when not applicable.</summary>
    public int? EntityId { get; set; }

    /// <summary>Serialized state before the change, or null for creations.</summary>
    public string? OldValue { get; set; }

    /// <summary>Serialized state after the change, or null for deletions.</summary>
    public string? NewValue { get; set; }

    /// <summary>When the operation happened (UTC).</summary>
    public DateTime Timestamp { get; set; }
}
