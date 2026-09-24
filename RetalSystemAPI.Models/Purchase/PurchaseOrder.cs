using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Models.Purchase;

/// <summary>
/// يمثل أمر وطلبية الشراء الصادرة من المنشأة إلى المورد لحجز وتوريد كميات محددة من البضاعة.
/// </summary>
// كلاس كيان أمر الشراء
public class PurchaseOrder : TenantBaseEntity
{
    /// <summary>
    /// رقم أمر الشراء الفريد في النظام (مثال: PO-0001).
    /// </summary>
    // خاصية رقم أمر الشراء الإلزامي
    public string OrderNumber { get; set; } = null!;

    /// <summary>
    /// معرّف المورد الموجه إليه أمر الشراء.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المورد
    public Guid? SupplierId { get; set; }

    /// <summary>
    /// كائن المورد المرتبط بأمر الشراء (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المورد
    public Supplier? Supplier { get; set; }

    /// <summary>
    /// معرّف المستودع أو صالة العرض المطلوب توريد واستلام الطلبية فيها.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المستودع
    public Guid? WarehouseId { get; set; }

    /// <summary>
    /// كائن المستودع المطلوب الاستلام فيه (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المستودع
    public Warehouse? Warehouse { get; set; }

    /// <summary>
    /// معرّف الفرع الذي تم إصدار أمر الشراء لصالحه.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الفرع
    public Guid BranchId { get; set; }

    /// <summary>
    /// كائن الفرع التابع له أمر الشراء (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الفرع
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// تاريخ ووقت تحرير وإنشاء أمر الشراء.
    /// </summary>
    // خاصية تاريخ إنشاء أمر الشراء
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// تاريخ الاستلام والتوريد المتوقع للبضاعة من المورد.
    /// </summary>
    // خاصية تاريخ التوريد المتوقع
    public DateTime? ExpectedDate { get; set; }

    /// <summary>
    /// حالة أمر الشراء الحالية (مسودة، مؤكد، مستلم جزئياً، مستلم، ملغى).
    /// </summary>
    // خاصية حالة أمر الشراء من تعداد PurchaseOrderStatus
    public PurchaseOrderStatus Status { get; set; }

    /// <summary>
    /// إجمالي القيمة التقديرية المتوقعة لأمر الشراء.
    /// </summary>
    // خاصية إجمالي مبلغ أمر الشراء
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// قائمة بنود وأصناف وكميات أمر الشراء المطلوب توريدها (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة بنود أمر الشراء
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
