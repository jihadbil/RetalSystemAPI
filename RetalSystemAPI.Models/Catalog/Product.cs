using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Models.Catalog;

/// <summary>
/// يمثل بيانات المنتجات والأصناف الأساسية في كتالوج النظام، بما في ذلك التسعير، التكلفة، التصنيف، والارتباط بالوحدات والباركودات.
/// </summary>
// كلاس كيان الصنف والمنتج الأساسي
public class Product : TenantBaseEntity
{
    /// <summary>
    /// الاسم التجاري والأساسي للصنف.
    /// </summary>
    // خاصية اسم المنتج الأساسي الإلزامي
    public required string Name { get; set; }

    /// <summary>
    /// الوصف والمواصفات التفصيلية للمنتج.
    /// </summary>
    // خاصية وصف الصنف الاختياري
    public string? Description { get; set; }

    /// <summary>
    /// سعر تكلفة الصنف الحالية، ويمثل أيضاً آخر سعر شراء تم اعتماده.
    /// </summary>
    // خاصية سعر التكلفة المباشر
    public decimal CostPrice { get; set; }

    /// <summary>
    /// سعر بيع الصنف القياسي للمستهلك.
    /// </summary>
    // خاصية سعر البيع الافتراضي
    public decimal SalePrice { get; set; }

    /// <summary>
    /// متوسط سعر تكلفة الشراء المرجح (Weighted Average Cost).
    /// </summary>
    // خاصية متوسط التكلفة المرجح
    public decimal AveragePrice { get; set; }

    /// <summary>
    /// معرّف التصنيف أو الفئة التي ينتمي إليها المنتج.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف التصنيف
    public Guid CategoryId { get; set; }

    /// <summary>
    /// كائن التصنيف التابع له المنتج (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان التصنيف
    public Category? Category { get; set; } = null;

    /// <summary>
    /// قائمة وحدات القياس والعبوات المعرفة لهذا المنتج مع معاملات التحويل (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة وحدات قياس الصنف
    public ICollection<ProductUnit> ProductUnits { get; set; } = new List<ProductUnit>();

    /// <summary>
    /// قائمة الباركودات المسجلة لهذا الصنف وعبواته (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة باركودات الصنف
    public ICollection<ProductBarCode> ProductBarCodes { get; set; } = new List<ProductBarCode>();

    /// <summary>
    /// قائمة الصور المرفوعة للمنتج (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة صور الصنف
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    /// <summary>
    /// قائمة أرصدة الصنف في صالات العرض ونقاط البيع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة أرصدة صالات العرض
    public ICollection<ShowroomStock> ShowroomStocks { get; set; } = new List<ShowroomStock>();
}
