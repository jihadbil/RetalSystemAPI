using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

/// <summary>
/// ناقل بيانات استجابة تفاصيل فاتورة المبيعات الشاملة (Sales Invoice Response DTO).
/// يتضمن البيانات المالية والفرع والعميل والملاحظات وقائمة البنود.
/// </summary>
public class SalesInvoiceResponseDto : BaseDto
{
    /// <summary>قائمة تفاصيل المدفوعات المجزأة المسجلة لنقطة البيع</summary>
    public List<RetalSystemAPI.Models.DTOs.Accounting.Pos.PosPaymentResponseDto> Payments { get; set; } = [];

    /// <summary>بيانات المنشأة والفرع المصدر للفاتورة وفق متطلبات الفوترة الضريبية</summary>
    public InvoiceIssuerDto? Issuer { get; set; }

    /// <summary>رقم فاتورة المبيعات الفريد</summary>
    public string InvoiceNumber { get; set; } = null!;

    /// <summary>تاريخ وتوقيت إصدار الفاتورة</summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>معرف الفرع المصدر</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>معرف المستودع أو الصالة المصروف منها الأصناف</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>اسم المستودع أو الصالة</summary>
    public string WarehouseName { get; set; } = null!;

    /// <summary>معرف العميل (إن وجد، أو فارغ للمبيعات النقدية العامة)</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>اسم العميل</summary>
    public string? CustomerName { get; set; }

    /// <summary>حالة الفاتورة (معلقة، معتمدة، ملغاة، إلخ)</summary>
    public InvoiceStatus Status { get; set; }

    /// <summary>اسم الحالة المعرب</summary>
    public string StatusName { get; set; } = null!;

    /// <summary>طريقة الدفع (نقدي، شبكة، آجل، مجزأ)</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>اسم طريقة الدفع المعرب</summary>
    public string PaymentMethodName { get; set; } = null!;

    /// <summary>المجموع الفرعي قبل الخصم والضريبة</summary>
    public decimal SubTotal { get; set; }

    /// <summary>قيمة الخصم الممنوح على إجمالي الفاتورة</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>إجمالي مبلغ ضريبة القيمة المضافة للفاتورة</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>المبلغ الإجمالي النهائي المستحق للسداد</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>المبلغ المسدد فعلياً من العميل</summary>
    public decimal PaidAmount { get; set; }

    /// <summary>المبلغ المتبقي آجل على العميل</summary>
    public decimal RemainingAmount { get; set; }

    /// <summary>معرف الخزينة المستلمة للدفعة النقدية</summary>
    public Guid? PaymentSafeId { get; set; }

    /// <summary>معرف نقطة البيع التي صدرت منها الفاتورة إن وجدت</summary>
    public Guid? PosTerminalId { get; set; }

    /// <summary>معرف جلسة الكاشير المرتبطة بالفاتورة</summary>
    public Guid? CashSessionId { get; set; }

    /// <summary>ملاحظات إضافية حول الفاتورة</summary>
    public string? Notes { get; set; }

    /// <summary>قائمة بنود الفاتورة التفصيلية</summary>
    public List<SalesInvoiceItemResponseDto> Items { get; set; } = new();
}
