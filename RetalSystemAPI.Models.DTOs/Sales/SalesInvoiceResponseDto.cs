using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

/// <summary>
/// ناقل بيانات استجابة تفاصيل فاتورة المبيعات الشاملة (Sales Invoice Response DTO).
/// يتضمن البيانات المالية والفرع والعميل والملاحظات وقائمة البنود.
/// </summary>
public class SalesInvoiceResponseDto : BaseDto
{
    /// <summary>رقم فاتورة المبيعات</summary>
    public string InvoiceNumber { get; set; } = null!;

    /// <summary>تاريخ وتوقيت إصدار الفاتورة</summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>معرف الفرع</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>معرف المستودع أو الصالة</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>اسم المستودع</summary>
    public string WarehouseName { get; set; } = null!;

    /// <summary>معرف العميل إن وجد</summary>
    public Guid? CustomerId { get; set; }

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

    /// <summary>المجموع الفرعي قبل الخصم</summary>
    public decimal SubTotal { get; set; }

    /// <summary>قيمة الخصم الممنوح</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>المبلغ الإجمالي الصافي</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>المبلغ المسدد</summary>
    public decimal PaidAmount { get; set; }

    /// <summary>المبلغ المتبقي آجل على العميل</summary>
    public decimal RemainingAmount { get; set; }

    /// <summary>ملاحظات إضافية</summary>
    public string? Notes { get; set; }

    /// <summary>قائمة بنود الفاتورة التفصيلية</summary>
    public List<SalesInvoiceItemResponseDto> Items { get; set; } = new();
}
