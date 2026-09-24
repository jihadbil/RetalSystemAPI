using System;
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Branchs;

/// <summary>
/// يمثل أرقام هواتف الاتصال المخصصة للفرع ومسؤولي التواصل داخله.
/// </summary>
// كلاس كيان رقم هاتف الفرع
public class BranchPhone : TenantBaseEntity    
{
    /// <summary>
    /// اسم الشخص أو القسم أو المسمى الوظيفي المرتبط برقم الهاتف (مثال: الإدارة، المبيعات).
    /// </summary>
    // خاصية اسم مسؤول الهاتف أو القسم
    public string? Name { get; set; }

    /// <summary>
    /// رقم الهاتف المدخل وفق النمط الليبي المعتمد.
    /// </summary>
    [Required]
    [RegularExpression(ValidationConstants.LibyanPhonePattern, ErrorMessage = ValidationConstants.LibyanPhoneError)]
    // خاصية رقم الهاتف الإلزامي
    public required string PhoneNumber { get; set; } 

    /// <summary>
    /// معرّف الفرع الذي ينتمي إليه رقم الهاتف هذا.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الفرع
    public Guid BranchId { get; set; }

    /// <summary>
    /// كائن الفرع المرتبط برقم الهاتف (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الفرع
    public Branch? Branch { get; set; }
}
