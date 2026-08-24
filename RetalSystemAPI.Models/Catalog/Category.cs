using System;
using System.Collections.Generic;
using System.Text;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Catalog;



/// <summary>
/// يمتل التصنيفات علي شكل هيكل هرمي بحيث يمكن ان يكون هناك تصنيف رئيسي و تصنيفات فرعية له
/// </summary>
public class Category : TenantBaseEntity
{
    /// <summary>
    /// اسم التصنيف
    /// </summary>
    public required string Name { get; set; } 
    /// <summary>
    /// معرف التصنيف الأب ان وجد
    /// </summary>
    public Guid? ParentCategoryId { get; set; }

    public Category? ParentCategory { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }  


    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
