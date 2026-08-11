using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Purchase;

public class UpdatePurchaseOrderDto
{
    [Required(ErrorMessage = "معرف الطلبية مطلوب")]
    public Guid Id { get; set; }

    public Guid? WarehouseId { get; set; }

    public DateTime? ExpectedDate { get; set; }

    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}
