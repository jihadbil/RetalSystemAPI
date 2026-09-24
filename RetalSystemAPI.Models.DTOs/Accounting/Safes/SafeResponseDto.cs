using System;
using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Accounting.Safes;

/// <summary>
/// ناقل بيانات استجابة تفاصيل الخزينة أو البنك (Safe Response DTO).
/// </summary>
public class SafeResponseDto : BaseDto
{
    /// <summary>اسم الخزينة أو الحساب المصرفي</summary>
    public string Name { get; set; } = null!;

    /// <summary>نوع الخزينة التعدادي</summary>
    public SafeType Type { get; set; }

    /// <summary>اسم نوع الخزينة بالعربية (بنك، صندوق، درج نقود)</summary>
    public string TypeName { get; set; } = null!;

    /// <summary>معرف الفرع التابعة له الخزينة</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>الرصيد الافتتاحي</summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>الرصيد الحالي المتوفر</summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>رقم الحساب المصرفي</summary>
    public string? BankAccountNumber { get; set; }

    /// <summary>اسم المصرف / البنك</summary>
    public string? BankName { get; set; }

    /// <summary>حالة نشاط وتشغيل الخزينة</summary>
    public bool IsActive { get; set; }

    /// <summary>ملاحظات إضافية</summary>
    public string? Notes { get; set; }
}
