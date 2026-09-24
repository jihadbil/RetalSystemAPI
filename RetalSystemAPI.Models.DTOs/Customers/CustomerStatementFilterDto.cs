using System;
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Customers;

/// <summary>
/// ناقل بيانات تصفية وفلترة كشف حساب العميل (Customer Statement Filter DTO).
/// </summary>
public class CustomerStatementFilterDto
{
    /// <summary>معرف العميل</summary>
    [Required(ErrorMessage = "معرف العميل مطلوب")]
    public Guid CustomerId { get; set; }

    /// <summary>تاريخ بداية الفترة (اختياري)</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>تاريخ نهاية الفترة (اختياري)</summary>
    public DateTime? ToDate { get; set; }

    /// <summary>معرف الفرع للفلترة (اختياري)</summary>
    public Guid? BranchId { get; set; }
}
