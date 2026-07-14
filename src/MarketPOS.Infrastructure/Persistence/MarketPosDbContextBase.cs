using MarketPOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPOS.Infrastructure.Persistence;

/// <summary>
/// Shared model definition for both the central (SQL Server) and local (SQLite)
/// databases — one schema, two providers, so synced data maps one-to-one.
/// </summary>
public abstract class MarketPosDbContextBase : DbContext
{
    /// <summary>Creates the context with provider-specific options.</summary>
    /// <param name="options">Options supplied by the derived context.</param>
    protected MarketPosDbContextBase(DbContextOptions options) : base(options)
    {
    }

    /// <summary>Product categories.</summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>Product catalog.</summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>Stock levels.</summary>
    public DbSet<Inventory> Inventories => Set<Inventory>();

    /// <summary>RBAC roles.</summary>
    public DbSet<Role> Roles => Set<Role>();

    /// <summary>System users.</summary>
    public DbSet<Employee> Employees => Set<Employee>();

    /// <summary>Cashier shifts.</summary>
    public DbSet<Shift> Shifts => Set<Shift>();

    /// <summary>Sale transactions.</summary>
    public DbSet<Sale> Sales => Set<Sale>();

    /// <summary>Sale receipt lines.</summary>
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    /// <summary>Append-only audit records.</summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100);
            // Restrict: deleting a parent must not cascade-delete a whole subtree.
            e.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.SKU).HasMaxLength(50);
            e.Property(p => p.Barcode).HasMaxLength(50);
            e.Property(p => p.Name).HasMaxLength(200);
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.Property(p => p.CostPrice).HasPrecision(18, 2);
            e.Property(p => p.TaxRate).HasPrecision(5, 4);
            e.HasIndex(p => p.SKU).IsUnique();
            e.HasIndex(p => p.Barcode).IsUnique();
            e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Inventory>(e =>
        {
            e.Property(i => i.QuantityOnHand).HasPrecision(18, 3);
            e.Property(i => i.ReorderLevel).HasPrecision(18, 3);
            e.HasIndex(i => i.ProductId).IsUnique();
            e.HasOne(i => i.Product)
                .WithOne(p => p.Inventory)
                .HasForeignKey<Inventory>(i => i.ProductId);
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.Property(r => r.Name).HasMaxLength(50);
            e.HasIndex(r => r.Name).IsUnique();
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.Property(emp => emp.FullName).HasMaxLength(150);
            e.Property(emp => emp.Username).HasMaxLength(50);
            e.Property(emp => emp.PasswordHash).HasMaxLength(200);
            e.HasIndex(emp => emp.Username).IsUnique();
            e.HasOne(emp => emp.Role)
                .WithMany(r => r.Employees)
                .HasForeignKey(emp => emp.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Shift>(e =>
        {
            e.Property(s => s.OpeningCash).HasPrecision(18, 2);
            e.Property(s => s.ClosingCash).HasPrecision(18, 2);
            e.Property(s => s.ExpectedCash).HasPrecision(18, 2);
            e.HasOne(s => s.Employee)
                .WithMany(emp => emp.Shifts)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Sale>(e =>
        {
            e.Property(s => s.TotalAmount).HasPrecision(18, 2);
            e.Property(s => s.TaxAmount).HasPrecision(18, 2);
            e.Property(s => s.DiscountAmount).HasPrecision(18, 2);
            // The sync service repeatedly asks for unsynced sales; index keeps that cheap.
            e.HasIndex(s => s.IsSyncedToServer);
            e.HasIndex(s => s.SaleDate);
            e.HasOne(s => s.Shift)
                .WithMany(sh => sh.Sales)
                .HasForeignKey(s => s.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SaleItem>(e =>
        {
            e.Property(si => si.ProductNameSnapshot).HasMaxLength(200);
            e.Property(si => si.UnitPriceSnapshot).HasPrecision(18, 2);
            e.Property(si => si.Quantity).HasPrecision(18, 3);
            e.Property(si => si.LineTotal).HasPrecision(18, 2);
            e.HasOne(si => si.Sale)
                .WithMany(s => s.Items)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
            // Restrict: products referenced by historical sales must not be hard-deleted.
            e.HasOne(si => si.Product)
                .WithMany()
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.Property(a => a.Action).HasMaxLength(100);
            e.Property(a => a.EntityName).HasMaxLength(100);
            e.HasIndex(a => a.Timestamp);
            e.HasOne(a => a.Employee)
                .WithMany()
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
