using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;

public class UpdateStockTransferDto
{
    [Required(ErrorMessage = "معرف أمر التحويل مطلوب")]
    public Guid Id { get; set; }

    public StockTransferStatus Status { get; set; }

    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}
