using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Accounting;

/// <summary>
/// يمثل تصنيفات وبنود المصروفات التبويبية في المنشأة (مثل: إيجارات، مرافق، صيانة، رواتب، مصاريف نقل).
/// </summary>
// كلاس كيان تصنيف المصروفات
public class ExpenseCategory : TenantBaseEntity
{
    /// <summary>
    /// مسمى تصنيف أو بند المصروف (مثال: كهرباء ومياه، صيانة دورية).
    /// </summary>
    // خاصية اسم تصنيف المصروف
    public required string Name { get; set; }

    /// <summary>
    /// حالة نشاط التصنيف وإمكانية إدراج سندات صرف تحته.
    /// </summary>
    // خاصية مؤشر حالة نشاط التصنيف
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// قائمة سندات المصروفات المقيدة تحت هذا التصنيف (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة سندات المصروفات
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
