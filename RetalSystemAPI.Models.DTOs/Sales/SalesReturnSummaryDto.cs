using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

/// <summary>
/// ناقل بيانات ملخص مرتجع المبيعات السريع (Sales Return Summary DTO).
/// خفيف ومخصص لعرض المرتجعات في الجداول وقوائم المتابعة والبحث.
/// </summary>
public class SalesReturnSummaryDto : BaseDto
{
    /// <summary>رقم إشعار الإرجاع</summary>
    public string ReturnNumber { get; set; } = null!;

    /// <summary>تاريخ الإرجاع</summary>
    public DateTime ReturnDate { get; set; }

    /// <summary>رقم فاتورة المبيعات الأصلية إن وجد</summary>
    public string? OriginalInvoiceNumber { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>اسم المستودع أو الصالة</summary>
    public string WarehouseName { get; set; } = null!;

    /// <summary>اسم العميل</summary>
    public string? CustomerName { get; set; }

    /// <summary>إجمالي قيمة المرتجع</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>سبب الإرجاع</summary>
    public SalesReturnReason Reason { get; set; }

    /// <summary>اسم سبب الإرجاع المعرب</summary>
    public string ReasonName { get; set; } = null!;

    /// <summary>عدد البنود المرتجعة</summary>
    public int ItemCount { get; set; }
}
