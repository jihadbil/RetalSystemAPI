using System;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Accounting.Safes;

/// <summary>
/// ناقل بيانات ملخص الخزينة للقوائم المنسدلة وشاشات العرض السريعة (Safe Summary DTO).
/// </summary>
public class SafeSummaryDto
{
    /// <summary>المعرف الفريد</summary>
    public Guid Id { get; set; }

    /// <summary>اسم الخزينة</summary>
    public string Name { get; set; } = null!;

    /// <summary>نوع الخزينة</summary>
    public SafeType Type { get; set; }

    /// <summary>اسم نوع الخزينة بالعربية</summary>
    public string TypeName { get; set; } = null!;

    /// <summary>معرف الفرع</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>الرصيد الحالي</summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>حالة النشاط</summary>
    public bool IsActive { get; set; }
}
