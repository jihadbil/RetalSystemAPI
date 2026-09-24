using System;
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Accounting.SafeTransactions;

/// <summary>
/// ناقل بيانات تحويل الأموال بين خزينتين أو حسابين مصرفيين (Transfer Funds DTO).
/// </summary>
public class TransferFundsDto
{
    /// <summary>معرف الخزينة المحول منها (المصدر)</summary>
    [Required(ErrorMessage = "الخزينة المصدر مطلوبة")]
    public Guid FromSafeId { get; set; }

    /// <summary>معرف الخزينة المحول إليها (الوجهة)</summary>
    [Required(ErrorMessage = "الخزينة الوجهة مطلوبة")]
    public Guid ToSafeId { get; set; }

    /// <summary>المبلغ المراد تحويله</summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "مبلغ التحويل يجب أن يكون أكبر من الصفر")]
    public decimal Amount { get; set; }

    /// <summary>بيان وشرح سبب التحويل</summary>
    [MaxLength(250, ErrorMessage = "البيان لا يمكن أن يتجاوز 250 حرفاً")]
    public string? Description { get; set; }

    /// <summary>ملاحظات إضافية</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}
