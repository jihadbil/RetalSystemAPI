using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.Models.Warehouses;

/// <summary>
/// يمثل المستودعات وصالات العرض التابعة للفروع في المنشأة، والتي تحتضن الأرصدة المخزنية للبضائع.
/// </summary>
// كلاس كيان المستودع وصالة العرض
public class Warehouse : TenantBaseEntity
{
    /// <summary>
    /// معرّف الفرع التابع له هذا المستودع أو صالة العرض.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الفرع
    public Guid BranchId { get; set; }

    /// <summary>
    /// كائن الفرع التابع له المستودع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الفرع
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// اسم المستودع أو صالة العرض (مثال: مستودع المواد الغذائية، صالة العرض رقم 1).
    /// </summary>
    // خاصية اسم المستودع الإلزامي
    public string Name { get; set; } = null!;

    /// <summary>
    /// نوع المستودع (مستودع تخزين Storge أو صالة عرض Show).
    /// </summary>
    // خاصية نوع المستودع من تعداد WarehouseType
    public WarehouseType Type { get; set; }

    /// <summary>
    /// حالة نشاط المستودع وإمكانية إدخال وإخراج البضائع منه.
    /// </summary>
    // خاصية مؤشر نشاط المستودع
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// قائمة أرصدة صالات العرض التابعة لهذا الموقع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة أرصدة صالات العرض
    public ICollection<ShowroomStock> ShowroomStocks { get; set; } = new List<ShowroomStock>();

    /// <summary>
    /// قائمة أرصدة مستودع التخزين التابعة لهذا الموقع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة أرصدة مستودع التخزين
    public ICollection<StorgeStock> WarehouseStocks { get; set; } = new List<StorgeStock>();

    /// <summary>
    /// قائمة أوامر الشراء الموجهة للاستلام في هذا المستودع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة أوامر الشراء المرتبطة
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
