using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

public class SalesReturnResponseDto : BaseDto
{
    public string ReturnNumber { get; set; } = null!;
    public DateTime ReturnDate { get; set; }
    public Guid? OriginalInvoiceId { get; set; }
    public string? OriginalInvoiceNumber { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public SalesReturnReason Reason { get; set; }
    public string ReasonName { get; set; } = null!;
    public string? Notes { get; set; }
    public List<SalesReturnItemResponseDto> Items { get; set; } = new();
}
