using System;
using System.Collections.Generic;
using System.Text;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Catalog;



/// <summary>
/// يمتل التصنيفات علي شكل هيكل هرمي بحيث يمكن ان يكون هناك تصنيف رئيسي و تصنيفات فرعية له
/// </summary>
public class Category:BaseEntity
{
    /// <summary>
    /// اسم التصنيف
    /// </summary>
    public string Name { get; set; } = null!;
    /// <summary>
    /// معرف التصنيف الأب ان وجد
    /// </summary>
    public int? ParentCategoryId { get; set; }

    public Category? ParentCategory { get; set; }

    /// <summary>
    /// معرف المستأجر
    /// </summary>
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
   // public ICollection<Product> Products { get; set; } = new List<Product>();
}
