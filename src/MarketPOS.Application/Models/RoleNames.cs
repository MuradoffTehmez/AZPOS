using MarketPOS.Domain.Entities;

namespace MarketPOS.Application.Models;

/// <summary>
/// Role name constants re-exposed for the UI layer, which talks only to
/// Application and must not reference Domain types directly.
/// </summary>
public static class RoleNames
{
    /// <summary>Cashier role.</summary>
    public const string Cashier = Role.Cashier;

    /// <summary>Store manager role.</summary>
    public const string Manager = Role.Manager;

    /// <summary>Administrator role.</summary>
    public const string Admin = Role.Admin;
}
