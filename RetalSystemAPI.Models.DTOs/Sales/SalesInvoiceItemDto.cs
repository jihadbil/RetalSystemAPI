using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Sales;

/// <summary>
/// ناقل بيانات بند فاتورة المبيعات (Sales Invoice Item DTO).
/// يتضمن الكمية وسعر البيع والتكلفة ومبلغ الخصم على مستوى الصنف.
/// </summary>
public class SalesInvoiceItemDto
{
    /// <summary>المعرف الفريد للمنتج</summary>
    [Required(ErrorMessage = "معرف الصنف مطلوب")]
    public Guid ProductId { get; set; }

    /// <summary>معرف الباركود المحدد للعبوة أو الصنف</summary>
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>الكمية المباعة</summary>
    [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون 1 على الأقل")]
    public int Quantity { get; set; }

    /// <summary>سعر بيع الوحدة</summary>
    [Range(0, double.MaxValue, ErrorMessage = "سعر الوحدة يجب أن يكون أكبر من أو يساوي 0")]
    public decimal UnitPrice { get; set; }

    /// <summary>تكلفة الوحدة وقت البيع لحساب الربحية بدقة</summary>
    [Range(0, double.MaxValue, ErrorMessage = "تكلفة الوحدة يجب أن تكون أكبر من أو يساوي 0")]
    public decimal UnitCost { get; set; }

    /// <summary>مبلغ الخصم الممنوح على هذا السطر</summary>
    [Range(0, double.MaxValue, ErrorMessage = "قيمة الخصم يجب أن تكون أكبر من أو تساوي 0")]
    public decimal DiscountAmount { get; set; } = 0;
}
