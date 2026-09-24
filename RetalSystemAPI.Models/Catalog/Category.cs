using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Catalog;

/// <summary>
/// يمثل شجرة وتصنيفات المنتجات والأصناف في النظام وفق هيكل شجري هرمي (تصنيفات رئيسية وفرعية).
/// </summary>
// كلاس كيان تصنيف الأصناف الشجري
public class Category : TenantBaseEntity
{
    /// <summary>
    /// مسمى التصنيف أو الفئة (مثال: إلكترونيات، هواتف، أجهزة منزلية).
    /// </summary>
    // خاصية اسم التصنيف الإلزامي
    public required string Name { get; set; } 

    /// <summary>
    /// معرّف التصنيف الأب الأعلى في الشجرة في حال كان هذا التصنيف فرعياً.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف التصنيف الأب
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// كائن التصنيف الأب المرتبط (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة للتصنيف الأب
    public Category? ParentCategory { get; set; }

    /// <summary>
    /// حالة نشاط التصنيف وإمكانية ربط منتجات به وعرضه في القوائم.
    /// </summary>
    // خاصية مؤشر حالة نشاط التصنيف
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// ترتيب عرض التصنيف في القوائم والشاشات التفاعلية ونقاط البيع.
    /// </summary>
    // خاصية ترتيب الفرز والعرض للتصنيف
    public int SortOrder { get; set; }  

    /// <summary>
    /// قائمة التصنيفات الفرعية المتفرعة من هذا التصنيف (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة التصنيفات الفرعية
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();

    /// <summary>
    /// قائمة المنتجات والأصناف المقيدة تحت هذا التصنيف (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة المنتجات التابعة للتصنيف
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
