using MarketPOS.Domain.Common;

namespace MarketPOS.Domain.Entities;

/// <summary>
/// Product category; supports an optional parent for hierarchical grouping.
/// </summary>
public class Category : EntityBase
{
    /// <summary>Display name of the category.</summary>
    public required string Name { get; set; }

    /// <summary>Parent category id, or null for a root category.</summary>
    public int? ParentCategoryId { get; set; }

    /// <summary>Parent category navigation.</summary>
    public Category? ParentCategory { get; set; }

    /// <summary>Direct child categories.</summary>
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();

    /// <summary>Products assigned to this category.</summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
