using RetalSystemAPI.Models.DTOs.Common;
using System;
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Accounting.Safes;

/// <summary>
/// ناقل بيانات إنشاء خزينة جديدة أو حساب بنكي أو درج نقود (Create Safe DTO).
/// </summary>
public class CreateSafeDto
{
    /// <summary>اسم الخزينة أو الحساب المصرفي أو الدرج</summary>
    [Required(ErrorMessage = "اسم الخزينة مطلوب")]
    [MaxLength(150, ErrorMessage = "اسم الخزينة لا يمكن أن يتجاوز 150 حرفاً")]
    public string Name { get; set; } = null!;

    /// <summary>نوع الخزينة (بنك، صندوق، درج نقود)</summary>
    [Required(ErrorMessage = "نوع الخزينة مطلوب")]
    [EnumDataType(typeof(SafeType))]
    public SafeType Type { get; set; } = SafeType.CashBox;

    /// <summary>الفرع التابعة له الخزينة</summary>
    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    [NotEmptyGuid]
    public Guid BranchId { get; set; }

    /// <summary>الرصيد الافتتاحي</summary>
    [Range(0, double.MaxValue, ErrorMessage = "الرصيد الافتتاحي يجب أن يكون أكبر من أو يساوي الصفر")]
    [MoneyAmount]
    public decimal OpeningBalance { get; set; } = 0;

    /// <summary>رقم الحساب المصرفي (للبنوك فقط)</summary>
    [MaxLength(50, ErrorMessage = "رقم الحساب المصرفي لا يمكن أن يتجاوز 50 حرفاً")]
    public string? BankAccountNumber { get; set; }

    /// <summary>اسم المصرف / البنك (للبنوك فقط)</summary>
    [MaxLength(100, ErrorMessage = "اسم المصرف لا يمكن أن يتجاوز 100 حرف")]
    public string? BankName { get; set; }

    /// <summary>حالة نشاط الخزينة (افتراضي: مفعل)</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>ملاحظات إضافية</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}
