using RetalSystemAPI.Models.DTOs.Common;
using System;
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

/// <summary>
/// ناقل بيانات تسوية رصيد حساب المورد (Supplier Balance Adjustment DTO).
/// </summary>
public class CreateSupplierAdjustmentDto
{
    /// <summary>معرف المورد</summary>
    [Required(ErrorMessage = "معرف المورد مطلوب")]
    [NotEmptyGuid]
    public Guid SupplierId { get; set; }

    /// <summary>معرف الفرع</summary>
    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    [NotEmptyGuid]
    public Guid BranchId { get; set; }

    /// <summary>اتجاه التسوية (وارد / زيادة التزام على المحل، أم صادر / تخفيض التزام)</summary>
    [Required(ErrorMessage = "اتجاه التسوية مطلوب")]
    [EnumDataType(typeof(TransactionDirection))]
    public TransactionDirection Direction { get; set; }

    /// <summary>مبلغ التسوية</summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "مبلغ التسوية يجب أن يكون أكبر من الصفر")]
    [MoneyAmount]
    public decimal Amount { get; set; }

    /// <summary>سبب وبيان التسوية</summary>
    [Required(ErrorMessage = "سبب وبيان التسوية مطلوب")]
    [MaxLength(250, ErrorMessage = "البيان لا يمكن أن يتجاوز 250 حرفاً")]
    public string Reason { get; set; } = null!;

    /// <summary>ملاحظات إضافية</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}
