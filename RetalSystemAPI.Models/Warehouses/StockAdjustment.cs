using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.Warehouses;

/// <summary>
/// يمثل سند التسوية الجردية لمعالجة الفوارق بين الكمية الدفترية المسجلة في النظام والكمية الفعلية على الرفوف (عجز أو زيادة أو إتلاف).
/// </summary>
// كلاس كيان سند التسوية الجردية للمخزون
public class StockAdjustment : TenantBaseEntity
{
    /// <summary>
    /// رقم سند التسوية الجردية الفريد التسلسلي الصادر بالنظام.
    /// </summary>
    // خاصية رقم سند التسوية الفريد
    public required string AdjustmentNumber { get; set; }

    /// <summary>
    /// تاريخ ووقت إجراء وتنفيذ التسوية الجردية.
    /// </summary>
    // خاصية تاريخ التسوية الجردية
    public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// معرّف المستودع أو صالة العرض التي تم فيها الجرد والتسوية.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المستودع
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// كائن المستودع أو الصالة التي تمت فيها التسوية (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المستودع
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// سبب التسوية الجردية من تعداد الأسباب المعتمدة (جرد دوري، تلف، منتهي الصلاحية، إلخ).
    /// </summary>
    // خاصية سبب التسوية من تعداد StockAdjustmentReason
    public StockAdjustmentReason Reason { get; set; }

    /// <summary>
    /// ملاحظات وإيضاحات وتبريرات إضافية حول سند التسوية الجردية.
    /// </summary>
    // خاصية ملاحظات سند التسوية
    public string? Notes { get; set; }

    /// <summary>
    /// قائمة بنود وأصناف التسوية الجردية والفروقات المحتسبة لكل صنف (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة بنود سند التسوية
    public ICollection<StockAdjustmentItem> Items { get; set; } = new List<StockAdjustmentItem>();
}
