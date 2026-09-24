using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Warehouses;

/// <summary>
/// يمثل رصيد الأصناف والمنتجات المعروضة في صالات العرض ونقاط البيع المباشرة (مرتبط بالصنف الأساسي ProductId).
/// </summary>
// كلاس كيان رصيد صالة العرض
public class ShowroomStock : TenantBaseEntity
{
    /// <summary>
    /// معرّف صالة العرض أو المستودع الذي توجد به البضاعة المعروضة.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف صالة العرض
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// كائن صالة العرض المرتبط (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان صالة العرض
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// معرّف الصنف والمنتج المعروض.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المنتج
    public Guid ProductId { get; set; }

    /// <summary>
    /// كائن المنتج المعروض في الصالة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المنتج
    public Product Product { get; set; } = null!;

    /// <summary>
    /// الكمية الفعلية المتوفرة حالياً في صالة العرض.
    /// </summary>
    // خاصية الكمية المتوفرة في الصالة
    public int Quantity { get; set; }

    /// <summary>
    /// حد الطلب الأدنى (نقطة إعادة الطلب) لإشعار المسؤول بضرورة تغذية الصالة من المخزن الرئيسي.
    /// </summary>
    // خاصية الحد الأدنى للمخزون
    public int MinStockLevel { get; set; }
}
