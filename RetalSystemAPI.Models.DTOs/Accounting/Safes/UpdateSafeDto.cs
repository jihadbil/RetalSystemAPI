using RetalSystemAPI.Models.DTOs.Common;
using System;
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Accounting.Safes;

/// <summary>
/// ناقل بيانات تعديل بيانات الخزينة أو الحساب المصرفي (Update Safe DTO).
/// </summary>
public class UpdateSafeDto
{
    /// <summary>اسم الخزينة أو الحساب المصرفي</summary>
    [Required(ErrorMessage = "اسم الخزينة مطلوب")]
    [MaxLength(150, ErrorMessage = "اسم الخزينة لا يمكن أن يتجاوز 150 حرفاً")]
    public string Name { get; set; } = null!;

    /// <summary>نوع الخزينة</summary>
    [Required(ErrorMessage = "نوع الخزينة مطلوب")]
    [EnumDataType(typeof(SafeType))]
    public SafeType Type { get; set; }

    /// <summary>رقم الحساب المصرفي (للبنوك فقط)</summary>
    [MaxLength(50, ErrorMessage = "رقم الحساب المصرفي لا يمكن أن يتجاوز 50 حرفاً")]
    public string? BankAccountNumber { get; set; }

    /// <summary>اسم المصرف / البنك (للبنوك فقط)</summary>
    [MaxLength(100, ErrorMessage = "اسم المصرف لا يمكن أن يتجاوز 100 حرف")]
    public string? BankName { get; set; }

    /// <summary>حالة نشاط الخزينة</summary>
    public bool IsActive { get; set; }

    /// <summary>ملاحظات إضافية</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}
