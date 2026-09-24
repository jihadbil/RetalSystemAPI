using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.Warehouses;

/// <summary>
/// يمثل سند ومناقلة تحويل البضائع والأصناف بين مستودعين أو صالتين (مثل التحويل من المخزن الرئيسي إلى صالة العرض أو بين الفروع).
/// </summary>
// كلاس كيان سند تحويل المخزون بين المستودعات
public class StockTransfer : TenantBaseEntity
{
    /// <summary>
    /// رقم أمر وسند التحويل المخزني الفريد الصادر بالنظام.
    /// </summary>
    // خاصية رقم سند التحويل الفريد
    public required string TransferNumber { get; set; }

    /// <summary>
    /// تاريخ ووقت إنشاء وتنفيذ أمر التحويل المخزني.
    /// </summary>
    // خاصية تاريخ أمر التحويل
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// معرّف المستودع أو صالة العرض المصدر المنقول منها البضاعة.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المستودع المصدر
    public Guid FromWarehouseId { get; set; }

    /// <summary>
    /// كائن المستودع أو الصالة المصدر (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المستودع المصدر
    public Warehouse FromWarehouse { get; set; } = null!;

    /// <summary>
    /// معرّف المستودع أو صالة العرض الوجهة المنقول إليها البضاعة.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المستودع الوجهة
    public Guid ToWarehouseId { get; set; }

    /// <summary>
    /// كائن المستودع أو الصالة الوجهة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المستودع الوجهة
    public Warehouse ToWarehouse { get; set; } = null!;

    /// <summary>
    /// حالة أمر التحويل الحالية (مسودة، مؤكدة، مكتملة ومرحلة، ملغاة).
    /// </summary>
    // خاصية حالة التحويل من تعداد StockTransferStatus
    public StockTransferStatus Status { get; set; } = StockTransferStatus.Draft;

    /// <summary>
    /// ملاحظات وإيضاحات إضافية حول سند التحويل المخزني.
    /// </summary>
    // خاصية ملاحظات سند التحويل
    public string? Notes { get; set; }

    /// <summary>
    /// قائمة بنود وأصناف وكميات البضاعة المحولة ضمن هذا السند (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة بنود سند التحويل
    public ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();
}
