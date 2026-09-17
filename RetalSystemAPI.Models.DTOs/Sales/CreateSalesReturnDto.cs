using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

/// <summary>
/// ناقل بيانات إنشاء مرتجع مبيعات جديد (Create Sales Return DTO).
/// يتضمن سبب الإرجاع، المستودع المستلم للبضاعة المعادة، وقائمة البنود المرتجعة.
/// </summary>
public class CreateSalesReturnDto
{
    /// <summary>رقم إشعار مرتجع المبيعات</summary>
    [Required(ErrorMessage = "رقم الإرجاع مطلوب")]
    [MaxLength(50, ErrorMessage = "رقم الإرجاع يجب أن لا يتجاوز 50 حرف")]
    public string ReturnNumber { get; set; } = null!;

    /// <summary>تاريخ وتوقيت الإرجاع</summary>
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

    /// <summary>معرف فاتورة المبيعات الأصلية إن وجد</summary>
    public Guid? OriginalInvoiceId { get; set; }

    /// <summary>معرف الفرع</summary>
    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }

    /// <summary>معرف المستودع أو الصالة المستلمة للبضاعة المرتجعة</summary>
    [Required(ErrorMessage = "معرف المستودع أو الصالة مطلوب")]
    public Guid WarehouseId { get; set; }

    /// <summary>معرف العميل</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>سبب الإرجاع (بضاعة معيبة، صنف خاطئ، تغيير رأي العميل، منتهي الصلاحية)</summary>
    public SalesReturnReason Reason { get; set; } = SalesReturnReason.Other;

    /// <summary>ملاحظات إضافية</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    /// <summary>قائمة البنود المرتجعة</summary>
    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل للمرتجع")]
    public List<SalesReturnItemDto> Items { get; set; } = new();
}
