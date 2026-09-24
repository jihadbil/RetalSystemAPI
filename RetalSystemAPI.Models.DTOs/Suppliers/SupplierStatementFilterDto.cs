using System;
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

/// <summary>
/// ناقل بيانات فلترة وتصفية كشف حساب المورد (Supplier Statement Filter DTO).
/// </summary>
public class SupplierStatementFilterDto
{
    /// <summary>معرف المورد</summary>
    [Required(ErrorMessage = "معرف المورد مطلوب")]
    public Guid SupplierId { get; set; }

    /// <summary>تاريخ بداية الفترة (اختياري)</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>تاريخ نهاية الفترة (اختياري)</summary>
    public DateTime? ToDate { get; set; }

    /// <summary>معرف الفرع للفلترة (اختياري)</summary>
    public Guid? BranchId { get; set; }
}
