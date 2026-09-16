using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Purchase;

public class CreatePurchaseOrderDto
{
    [Required(ErrorMessage = "رقم الطلبية مطلوب")]
    [MaxLength(50, ErrorMessage = "رقم الطلبية يجب أن لا يتجاوز 50 حرف")]
    public string OrderNumber { get; set; } = null!;

    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }

    public Guid? SupplierId { get; set; }

    public Guid? WarehouseId { get; set; }

    [Required(ErrorMessage = "تاريخ الطلبية مطلوب")]
    public DateTime OrderDate { get; set; }

    public DateTime? ExpectedDate { get; set; }

    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل")]
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}
