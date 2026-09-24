using System;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Customers;

/// <summary>
/// يمثل أرقام هواتف التواصل وجهات الاتصال المتعددة المسجلة لحساب العميل.
/// </summary>
// كلاس كيان رقم هاتف العميل
public class CustomerPhone : TenantBaseEntity
{
    /// <summary>
    /// معرّف العميل الذي ينتمي إليه رقم الهاتف هذا.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف العميل
    public Guid CustomerId { get; set; }

    /// <summary>
    /// كائن العميل المرتبط برقم الهاتف (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان العميل
    public Customer Customer { get; set; } = null!;

    /// <summary>
    /// رقم الهاتف المسجل للتواصل.
    /// </summary>
    // خاصية رقم الهاتف الإلزامي
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// اسم جهة الاتصال أو مسؤول المشتريات/المندوب التابع للعميل (اختياري).
    /// </summary>
    // خاصية اسم صاحب الرقم أو جهة الاتصال
    public string? ContactName { get; set; }

    /// <summary>
    /// يحدد ما إذا كان هذا هو رقم الهاتف الافتراضي والرئيسي للتواصل.
    /// </summary>
    // خاصية مؤشر رقم الهاتف الافتراضي
    public bool IsDefault { get; set; } = false;
}
