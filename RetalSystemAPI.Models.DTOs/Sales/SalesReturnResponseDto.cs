using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

/// <summary>
/// ناقل بيانات استجابة تفاصيل مرتجع المبيعات الشاملة (Sales Return Response DTO).
/// </summary>
public class SalesReturnResponseDto : BaseDto
{
    /// <summary>رقم إشعار الإرجاع</summary>
    public string ReturnNumber { get; set; } = null!;

    /// <summary>تاريخ وتوقيت الإرجاع</summary>
    public DateTime ReturnDate { get; set; }

    /// <summary>معرف فاتورة المبيعات الأصلية</summary>
    public Guid? OriginalInvoiceId { get; set; }

    /// <summary>رقم فاتورة المبيعات الأصلية</summary>
    public string? OriginalInvoiceNumber { get; set; }

    /// <summary>معرف الفرع</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>معرف المستودع أو الصالة المستلمة</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>اسم المستودع</summary>
    public string WarehouseName { get; set; } = null!;

    /// <summary>معرف العميل</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>اسم العميل</summary>
    public string? CustomerName { get; set; }

    /// <summary>إجمالي المبلغ المرتجع</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>سبب الإرجاع التعدادي</summary>
    public SalesReturnReason Reason { get; set; }

    /// <summary>اسم سبب الإرجاع المعرب</summary>
    public string ReasonName { get; set; } = null!;

    /// <summary>ملاحظات إضافية</summary>
    public string? Notes { get; set; }

    /// <summary>قائمة بنود المرتجع المفصلة</summary>
    public List<SalesReturnItemResponseDto> Items { get; set; } = new();
}
