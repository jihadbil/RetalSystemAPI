using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Customers;

public class CustomerPhoneResponseDto : BaseDto
{
    public Guid CustomerId { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string? ContactName { get; set; }
    public bool IsDefault { get; set; }
}
