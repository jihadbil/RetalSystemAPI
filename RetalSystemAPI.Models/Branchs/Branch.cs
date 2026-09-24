using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Models.Branchs;

/// <summary>
/// يمثل كيان الفرع في النظام، ويحتوي على الخصائص المتعلقة بالفرع مثل الاسم، العنوان، وحالة النشاط، وروابط المستخدمين والمستودعات.
/// </summary>
// كلاس كيان الفرع التجاري
public class Branch : TenantBaseEntity
{
    /// <summary>
    /// اسم الفرع التجاري (مثال: الفرع الرئيسي، فرع طرابلس).
    /// </summary>
    // خاصية اسم الفرع
    public string Name { get; set; } = null!;

    /// <summary>
    /// العنوان الجغرافي التفصيلي لموقع الفرع.
    /// </summary>
    // خاصية عنوان الفرع الجغرافي
    public string? Address { get; set; }

    /// <summary>
    /// حالة نشاط الفرع وإمكانية تسجيل حركات ومبيعات عليه.
    /// </summary>
    // خاصية مؤشر حالة نشاط الفرع
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// قائمة أرقام هواتف الاتصال المخصصة للفرع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة أرقام هواتف الفرع
    public ICollection<BranchPhone> BranchPhones { get; set; } = new List<BranchPhone>();

    /// <summary>
    /// قائمة موظفي ومستخدمي النظام المعينين في هذا الفرع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة مستخدمي الفرع
    public ICollection<ApplicationUser> ApplicationUsers { get; set; } = new List<ApplicationUser>();

    /// <summary>
    /// قائمة المستودعات وصالات العرض التابعة لهذا الفرع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة مستودعات الفرع
    public ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>(); 

    /// <summary>
    /// قائمة أوامر الشراء المرتبطة والمطلوبة لهذا الفرع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة أوامر الشراء التابعة للفرع
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
