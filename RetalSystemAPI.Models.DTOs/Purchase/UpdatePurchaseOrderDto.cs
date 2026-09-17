using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Purchase;

/// <summary>
/// ناقل بيانات تعديل أمر شراء قائم (Update Purchase Order DTO).
/// </summary>
public class UpdatePurchaseOrderDto
{
    /// <summary>المعرف الفريد لأمر الشراء المطلوب تعديله</summary>
    [Required(ErrorMessage = "معرف الطلبية مطلوب")]
    public Guid Id { get; set; }

    /// <summary>معرف المورد</summary>
    public Guid? SupplierId { get; set; }

    /// <summary>معرف المستودع</summary>
    public Guid? WarehouseId { get; set; }

    /// <summary>التاريخ المتوقع للتوريد</summary>
    public DateTime? ExpectedDate { get; set; }

    /// <summary>قائمة بنود الطلبية المحدثة</summary>
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}
