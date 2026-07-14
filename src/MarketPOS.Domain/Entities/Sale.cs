using MarketPOS.Domain.Common;
using MarketPOS.Domain.Enums;

namespace MarketPOS.Domain.Entities;

/// <summary>
/// Sale transaction (receipt header). Written offline-first: persisted locally
/// with <see cref="IsSyncedToServer"/> = false, pushed to the central server by
/// the sync background service.
/// </summary>
public class Sale : EntityBase
{
    /// <summary>Shift the sale belongs to.</summary>
    public int ShiftId { get; set; }

    /// <summary>Shift navigation.</summary>
    public Shift? Shift { get; set; }

    /// <summary>Cashier who performed the sale.</summary>
    public int EmployeeId { get; set; }

    /// <summary>Employee navigation.</summary>
    public Employee? Employee { get; set; }

    /// <summary>Timestamp of the transaction.</summary>
    public DateTime SaleDate { get; set; }

    /// <summary>Final amount paid, taxes included, discounts applied.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Total tax (ƏDV) portion of the sale.</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>Total discount applied across the receipt.</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>How the sale was paid.</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Lifecycle state of the transaction.</summary>
    public SaleStatus Status { get; set; } = SaleStatus.Completed;

    /// <summary>False until the sync service confirms the central server has the record.</summary>
    public bool IsSyncedToServer { get; set; }

    /// <summary>Receipt lines.</summary>
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}
