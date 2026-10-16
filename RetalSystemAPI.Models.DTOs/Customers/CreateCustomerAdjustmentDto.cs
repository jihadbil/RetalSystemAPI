using RetalSystemAPI.Models.DTOs.Common;
using System;
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Customers;

/// <summary>
/// ناقل بيانات تسوية رصيد حساب العميل (Customer Balance Adjustment DTO).
/// </summary>
public class CreateCustomerAdjustmentDto
{
    /// <summary>معرف العميل</summary>
    [Required(ErrorMessage = "معرف العميل مطلوب")]
    [NotEmptyGuid]
    public Guid CustomerId { get; set; }

    /// <summary>معرف الفرع</summary>
    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    [NotEmptyGuid]
    public Guid BranchId { get; set; }

    /// <summary>اتجاه التسوية (وارد / زيادة دين، أم صادر / تخفيض دين)</summary>
    [Required(ErrorMessage = "اتجاه التسوية مطلوب")]
    [EnumDataType(typeof(TransactionDirection))]
    public TransactionDirection Direction { get; set; }

    /// <summary>مبلغ التسوية</summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "مبلغ التسوية يجب أن يكون أكبر من الصفر")]
    [MoneyAmount]
    public decimal Amount { get; set; }

    /// <summary>سبب وبيان التسوية</summary>
    [Required(ErrorMessage = "سبب التسوية مطلوب")]
    [MaxLength(250, ErrorMessage = "البيان لا يمكن أن يتجاوز 250 حرفاً")]
    public string Reason { get; set; } = null!;

    /// <summary>ملاحظات إضافية</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}
