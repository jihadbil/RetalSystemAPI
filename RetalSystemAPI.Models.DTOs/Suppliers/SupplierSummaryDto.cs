using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

public class SupplierSummaryDto : BaseDto
{
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public decimal OpeningBalance { get; set; }
    public int PhoneCount { get; set; }
}
