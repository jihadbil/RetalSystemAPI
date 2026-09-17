using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

/// <summary>
/// ناقل بيانات ملخص فاتورة المبيعات السريع (Sales Invoice Summary DTO).
/// خفيف ومخصص لعرض الفواتير في الجداول وقوائم البحث السريع.
/// </summary>
public class SalesInvoiceSummaryDto : BaseDto
{
    /// <summary>رقم فاتورة المبيعات</summary>
    public string InvoiceNumber { get; set; } = null!;

    /// <summary>تاريخ إصدار الفاتورة</summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>اسم المستودع</summary>
    public string WarehouseName { get; set; } = null!;

    /// <summary>اسم العميل</summary>
    public string? CustomerName { get; set; }

    /// <summary>حالة الفاتورة</summary>
    public InvoiceStatus Status { get; set; }

    /// <summary>اسم الحالة المعرب</summary>
    public string StatusName { get; set; } = null!;

    /// <summary>طريقة الدفع</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>اسم طريقة الدفع المعرب</summary>
    public string PaymentMethodName { get; set; } = null!;

    /// <summary>المبلغ الإجمالي</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>المبلغ المدفوع</summary>
    public decimal PaidAmount { get; set; }

    /// <summary>المبلغ المتبقي</summary>
    public decimal RemainingAmount { get; set; }

    /// <summary>عدد بنود الفاتورة</summary>
    public int ItemCount { get; set; }
}
