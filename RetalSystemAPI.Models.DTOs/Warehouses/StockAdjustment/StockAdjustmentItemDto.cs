using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;

public class StockAdjustmentItemDto
{
    [Required(ErrorMessage = "معرف الصنف مطلوب")]
    public Guid ProductId { get; set; }

    public Guid? ProductBarCodeId { get; set; }

    public int SystemQuantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "الكمية الفعلية يجب أن تكون أكبر من أو تساوي 0")]
    public int ActualQuantity { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "تكلفة الوحدة يجب أن تكون أكبر من أو تساوي 0")]
    public decimal UnitCost { get; set; }
}
