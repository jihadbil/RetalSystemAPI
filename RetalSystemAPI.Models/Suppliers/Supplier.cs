using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Suppliers;

/// <summary>
/// يمثل بيانات الموردين والشركات الموردة للبضائع والأصناف في النظام، متضمناً الأرصدة الدفترية وسجلات الاتصال.
/// </summary>
// كلاس كيان المورد التجاري
public class Supplier : TenantBaseEntity
{
    /// <summary>
    /// اسم المورد التجاري أو اسم الشركة والمؤسسة الموردة.
    /// </summary>
    // خاصية اسم المورد الإلزامي
    public string Name { get; set; } = null!;

    /// <summary>
    /// العنوان الجغرافي أو مقر مستودعات ومكاتب المورد.
    /// </summary>
    // خاصية عنوان مقر المورد
    public string? Address { get; set; }

    /// <summary>
    /// الرصيد الافتتاحي المبدئي المستحق للمورد عند تدشين التعامل في النظام.
    /// </summary>
    // خاصية الرصيد الافتتاحي للمورد
    public decimal OpeningBalance { get; set; } = 0;

    /// <summary>
    /// الرصيد الحالي الصافي للمورد (يُحدَّث آلياً مع كل فاتورة شراء وسداد ومرتجع).
    /// </summary>
    // خاصية الرصيد الدفتري الحالي للمورد
    public decimal CurrentBalance { get; set; } = 0;

    /// <summary>
    /// حالة نشاط المورد وإمكانية إصدار أوامر وفواتير شراء جديدة منه.
    /// </summary>
    // خاصية مؤشر حالة نشاط المورد
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// قائمة أرقام هواتف التواصل المسجلة للمورد ومنسقي المبيعات لديه (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة أرقام هواتف المورد
    public ICollection<SupplierPhone> SupplierPhones { get; set; } = new List<SupplierPhone>();

    /// <summary>
    /// سجل الحركات المالية ودفتر أستاذ حساب المورد (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة الحركات المالية للمورد
    public ICollection<SupplierTransaction> SupplierTransactions { get; set; } = new List<SupplierTransaction>();
}
