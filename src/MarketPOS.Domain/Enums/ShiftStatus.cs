namespace MarketPOS.Domain.Enums;

/// <summary>
/// Lifecycle state of a cashier shift.
/// </summary>
public enum ShiftStatus
{
    /// <summary>Shift is open; sales can be registered against it.</summary>
    Open = 0,

    /// <summary>Shift is closed; closing cash has been declared and reconciled.</summary>
    Closed = 1
}
