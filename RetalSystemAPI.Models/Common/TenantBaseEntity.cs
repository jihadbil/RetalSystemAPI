namespace RetalSystemAPI.Models.Common;

/// <summary>
/// الكيان الأساسي التجريدي لكافة الجداول والنماذج التابعة لمستأجر محدد في النظام لضمان العزل التام للبيانات (Multi-Tenancy).
/// يرث الخصائص المشتركة من BaseEntity ويضيف معرّف المستأجر.
/// </summary>
// الكلاس الأساسي للكيانات متعددة المستأجرين
public abstract class TenantBaseEntity : BaseEntity
{
    /// <summary>
    /// معرّف المستأجر (المؤسسة) الذي ينتمي إليه هذا السجل لضمان عزل البيانات.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المستأجر
    public Guid TenantId { get; set; }

    /// <summary>
    /// الكائن المرجعي لبيانات المستأجر المرتبط بهذا السجل (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المستأجر
    public Tenant? Tenant { get; set; }
}
