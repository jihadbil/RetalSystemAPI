using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

public class SalesReturnSummaryDto : BaseDto
{
    public string ReturnNumber { get; set; } = null!;
    public DateTime ReturnDate { get; set; }
    public string? OriginalInvoiceNumber { get; set; }
    public string BranchName { get; set; } = null!;
    public string WarehouseName { get; set; } = null!;
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public SalesReturnReason Reason { get; set; }
    public string ReasonName { get; set; } = null!;
    public int ItemCount { get; set; }
}
