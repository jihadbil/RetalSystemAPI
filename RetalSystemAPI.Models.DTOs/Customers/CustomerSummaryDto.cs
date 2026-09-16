using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Customers;

public class CustomerSummaryDto : BaseDto
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public CustomerType Type { get; set; }
    public string TypeName { get; set; } = null!;
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public int PhoneCount { get; set; }
    public string? PrimaryPhone { get; set; }
}
