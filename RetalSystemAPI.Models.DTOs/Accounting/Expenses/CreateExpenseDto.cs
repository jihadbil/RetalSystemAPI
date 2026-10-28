using RetalSystemAPI.Models.DTOs.Common;
using System;
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Accounting.Expenses;

/// <summary>
/// ناقل بيانات تسجيل سند مصروف تشغيلي جديد (Create Expense DTO).
/// </summary>
public class CreateExpenseDto
{
    /// <summary>تاريخ المصروف</summary>
    [Required(ErrorMessage = "تاريخ المصروف مطلوب")]
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

    /// <summary>الفرع الذي تم فيه الصرف</summary>
    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    [NotEmptyGuid]
    public Guid BranchId { get; set; }

    /// <summary>معرف تصنيف المصروف</summary>
    [Required(ErrorMessage = "تصنيف المصروف مطلوب")]
    [NotEmptyGuid]
    public Guid ExpenseCategoryId { get; set; }

    /// <summary>مبلغ المصروف</summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "مبلغ المصروف يجب أن يكون أكبر من الصفر")]
    [MoneyAmount]
    public decimal Amount { get; set; }

    /// <summary>طريقة الدفع (نقدي، تحويل مصرفي، بطاقة...)</summary>
    [Required(ErrorMessage = "طريقة الدفع مطلوبة")]
    [SinglePaymentMethod]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    /// <summary>معرف الخزينة أو الدرج المسحوب منه المبلغ (اختياري / مطلوب في الدفع النقدي والبنكي)</summary>
    public Guid? SafeId { get; set; }

    /// <summary>اسم المستفيد أو الجهة المصروف لها</summary>
    [MaxLength(150, ErrorMessage = "اسم المستفيد لا يمكن أن يتجاوز 150 حرفاً")]
    public string? BeneficiaryName { get; set; }

    /// <summary>البيان / تفاصيل وشرح المصروف</summary>
    [Required(ErrorMessage = "بيان المصروف مطلوب")]
    [MaxLength(300, ErrorMessage = "بيان المصروف لا يمكن أن يتجاوز 300 حرف")]
    public string Description { get; set; } = null!;

    /// <summary>ملاحظات إضافية</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}
