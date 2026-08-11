using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

public class SupplierPhoneResponseDto : BaseDto
{
    public string? Name { get; set; }
    public string PhoneNumber { get; set; } = null!;
}
