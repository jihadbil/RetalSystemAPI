using System;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Suppliers;

/// <summary>
/// يمثل أرقام هواتف التواصل وجهات الاتصال المتعددة التابعة للمورد ومسؤولي التوريد.
/// </summary>
// كلاس كيان رقم هاتف المورد
public class SupplierPhone : TenantBaseEntity
{
    /// <summary>
    /// معرّف المورد الذي ينتمي إليه رقم الهاتف هذا.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المورد
    public Guid SupplierId { get; set; }

    /// <summary>
    /// كائن المورد المرتبط برقم الهاتف (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المورد
    public Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// رقم الهاتف المسجل للمورد.
    /// </summary>
    // خاصية رقم الهاتف الإلزامي
    public string PhoneNumber { get; set; } = null!;

    /// <summary>
    /// اسم جهة الاتصال أو مسؤول المبيعات لدى المورد (مثال: مندوب التوزيع، المحاسب).
    /// </summary>
    // خاصية اسم صاحب الرقم أو جهة الاتصال
    public string? Name { get; set; }
}
