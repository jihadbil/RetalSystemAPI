using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Purchase;

/// <summary>
/// ناقل بيانات إنشاء طلبية أو أمر شراء جديد (Create Purchase Order DTO).
/// </summary>
public class CreatePurchaseOrderDto
{
    /// <summary>رقم أمر الشراء</summary>
    [Required(ErrorMessage = "رقم الطلبية مطلوب")]
    [MaxLength(50, ErrorMessage = "رقم الطلبية يجب أن لا يتجاوز 50 حرف")]
    public string OrderNumber { get; set; } = null!;

    /// <summary>معرف الفرع الطالب</summary>
    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }

    /// <summary>معرف المورد المستهدف</summary>
    public Guid? SupplierId { get; set; }

    /// <summary>معرف المستودع المستلم</summary>
    public Guid? WarehouseId { get; set; }

    /// <summary>تاريخ إصدار أمر الشراء</summary>
    [Required(ErrorMessage = "تاريخ الطلبية مطلوب")]
    public DateTime OrderDate { get; set; }

    /// <summary>تاريخ التوريد المتوقع</summary>
    public DateTime? ExpectedDate { get; set; }

    /// <summary>قائمة بنود الأصناف المطلوبة في أمر الشراء</summary>
    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل")]
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}
