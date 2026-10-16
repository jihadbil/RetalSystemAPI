using RetalSystemAPI.Models.DTOs.Common;
using System;
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Accounting.SafeTransactions;

/// <summary>
/// ناقل بيانات تسجيل حركة خزينة يدوية (إيداع أو سحب نقدي).
/// </summary>
public class CreateSafeTransactionDto
{
    /// <summary>معرف الخزينة المستهدفة</summary>
    [Required(ErrorMessage = "معرف الخزينة مطلوب")]
    [NotEmptyGuid]
    public Guid SafeId { get; set; }

    /// <summary>اتجاه الحركة (وارد / إيداع أو صادر / سحب)</summary>
    [Required(ErrorMessage = "اتجاه الحركة مطلوب")]
    [EnumDataType(typeof(TransactionDirection))]
    public TransactionDirection Direction { get; set; }

    /// <summary>مبلغ الحركة</summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "مبلغ الحركة يجب أن يكون أكبر من الصفر")]
    [MoneyAmount]
    public decimal Amount { get; set; }

    /// <summary>البيان / الشرح التفصيلي لسبب الحركة</summary>
    [Required(ErrorMessage = "بيان الحركة مطلوب")]
    [MaxLength(250, ErrorMessage = "البيان لا يمكن أن يتجاوز 250 حرفاً")]
    public string Description { get; set; } = null!;

    /// <summary>ملاحظات إضافية</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}
