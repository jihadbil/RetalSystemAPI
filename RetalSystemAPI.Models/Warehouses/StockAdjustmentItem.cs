using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.Warehouses;

/// <summary>
/// يمثل بنداً واحداً تفصيلياً ضمن بنود سند التسوية الجردية، موضحاً الكمية الدفترية، الفعلية، وفارق العجز أو الزيادة.
/// </summary>
// كلاس كيان بند سند التسوية الجردية
public class StockAdjustmentItem : TenantBaseEntity
{
    /// <summary>
    /// معرّف سند التسوية الجردية الأساسي الذي يتبع له هذا البند.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف سند التسوية
    public Guid StockAdjustmentId { get; set; }

    /// <summary>
    /// كائن سند التسوية الجردية المرتبط (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان سند التسوية
    public StockAdjustment StockAdjustment { get; set; } = null!;

    /// <summary>
    /// معرّف المنتج والصنف الذي تتم تسويته.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المنتج
    public Guid ProductId { get; set; }

    /// <summary>
    /// كائن المنتج المرتبط بالبند (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المنتج
    public Product Product { get; set; } = null!;

    /// <summary>
    /// معرّف الباركود أو العبوة المحددة الخاضعة للتسوية (اختياري في حال صالات العرض).
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف باركود الصنف
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>
    /// كائن باركود الصنف المرتبط بالبند (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان باركود الصنف
    public ProductBarCode? ProductBarCode { get; set; }

    /// <summary>
    /// الكمية الدفترية المسجلة في النظام قبل إجراء التسوية الجردية.
    /// </summary>
    // خاصية الكمية المسجلة دفترياً بالنظام
    public int SystemQuantity { get; set; }

    /// <summary>
    /// الكمية الفعلية المحصورة بعد الجرد الواقعي في المستودع.
    /// </summary>
    // خاصية الكمية الفعلية المحصورة بالجرد
    public int ActualQuantity { get; set; }

    /// <summary>
    /// الفارق المحسوب (الفعلي - الدفتري): قيمة موجبة تعني زيادة، وسالبة تعني عجزاً.
    /// </summary>
    // خاصية فرق الكمية المحسوب
    public int DifferenceQuantity { get; set; }

    /// <summary>
    /// تكلفة الوحدة الواحدة لحساب الأثر والقيمة المالية الإجمالية للتسوية.
    /// </summary>
    // خاصية سعر تكلفة الوحدة
    public decimal UnitCost { get; set; }

    /// <summary>
    /// سبب تسوية هذا البند تحديداً (جرد دوري، تلف، انتهاء صلاحية) مستقلاً عن سبب السند العام.
    /// </summary>
    // خاصية سبب تسوية البند من تعداد StockAdjustmentReason
    public StockAdjustmentReason Reason { get; set; } = StockAdjustmentReason.InventoryCount;
}
