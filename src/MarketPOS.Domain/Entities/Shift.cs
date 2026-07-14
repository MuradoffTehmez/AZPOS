using MarketPOS.Domain.Common;
using MarketPOS.Domain.Enums;

namespace MarketPOS.Domain.Entities;

/// <summary>
/// Cashier work shift. Opening/closing cash declarations feed the X/Z reports
/// and the cash reconciliation at shift close.
/// </summary>
public class Shift : EntityBase
{
    /// <summary>Employee who owns the shift.</summary>
    public int EmployeeId { get; set; }

    /// <summary>Employee navigation.</summary>
    public Employee? Employee { get; set; }

    /// <summary>When the shift was opened.</summary>
    public DateTime OpenedAt { get; set; }

    /// <summary>When the shift was closed, or null while open.</summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>Cash declared in the drawer at opening.</summary>
    public decimal OpeningCash { get; set; }

    /// <summary>Cash counted at closing, or null while open.</summary>
    public decimal? ClosingCash { get; set; }

    /// <summary>System-expected cash at closing (opening + cash sales), or null while open.</summary>
    public decimal? ExpectedCash { get; set; }

    /// <summary>Current lifecycle state.</summary>
    public ShiftStatus Status { get; set; } = ShiftStatus.Open;

    /// <summary>Sales registered during this shift.</summary>
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
