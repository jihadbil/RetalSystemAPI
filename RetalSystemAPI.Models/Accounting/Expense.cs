using System;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.Accounting;

/// <summary>
/// يمثل سندات المصروفات وسجلات الإنفاق التشغيلي والإداري في الفروع والمؤسسة.
/// </summary>
// كلاس كيان سند المصروف المالي
public class Expense : TenantBaseEntity
{
    /// <summary>
    /// رقم سند المصروف الفريد التسلسلي الصادر بالنظام.
    /// </summary>
    // خاصية رقم سند المصروف الفريد
    public required string ExpenseNumber { get; set; }

    /// <summary>
    /// تاريخ ووقت تسجيل أو دفع المصروف الفعلي.
    /// </summary>
    // خاصية تاريخ سند المصروف
    public DateTime ExpenseDate { get; set; }

    /// <summary>
    /// معرّف الفرع الذي تم تسجيل وصرف المصروف فيه.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الفرع
    public Guid BranchId { get; set; }

    /// <summary>
    /// كائن الفرع الذي تم فيه الصرف (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الفرع
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// معرّف بند أو تصنيف المصروف المحاسبي.
    /// </summary>
    // خاصية المفتاح الأجنبي لتصنيف المصروف
    public Guid ExpenseCategoryId { get; set; }

    /// <summary>
    /// كائن تصنيف المصروف المحاسبي (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان تصنيف المصروف
    public ExpenseCategory ExpenseCategory { get; set; } = null!;

    /// <summary>
    /// القيمة المالية المدفوعة للمصروف.
    /// </summary>
    // خاصية مبلغ المصروف
    public decimal Amount { get; set; }

    /// <summary>
    /// طريقة السداد والوسيلة المستخدمة لدفع قيمة المصروف.
    /// </summary>
    // خاصية طريقة الدفع من تعداد PaymentMethod
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    /// <summary>
    /// معرّف الخزينة أو الحساب البنكي أو الدرج المسحوب منه المبلغ.
    /// </summary>
    // خاصية المفتاح الأجنبي للخزينة المسحوب منها
    public Guid? SafeId { get; set; }

    /// <summary>
    /// كائن الخزينة المسحوب منها مبلغ المصروف (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الخزينة
    public Safe? Safe { get; set; }

    /// <summary>
    /// اسم المستفيد أو الشخص أو الجهة الخارجية المصروف لها المبلغ.
    /// </summary>
    // خاصية اسم المستفيد من المصروف
    public string? BeneficiaryName { get; set; }

    /// <summary>
    /// البيان والشرح التفصيلي لسبب الصرف وبنود التكلفة.
    /// </summary>
    // خاصية البيان الإلزامي للمصروف
    public required string Description { get; set; }

    /// <summary>
    /// ملاحظات وإيضاحات إضافية حول سند المصروف.
    /// </summary>
    // خاصية الملاحظات العامة لسند المصروف
    public string? Notes { get; set; }
}
