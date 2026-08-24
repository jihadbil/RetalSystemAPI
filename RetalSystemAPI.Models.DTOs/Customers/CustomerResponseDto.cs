using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Customers;

public class CustomerResponseDto : BaseDto
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public CustomerType Type { get; set; }
    public string TypeName { get; set; } = null!;
    public decimal OpeningBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public List<CustomerPhoneResponseDto> Phones { get; set; } = new();
}
