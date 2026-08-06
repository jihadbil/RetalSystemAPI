using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Branch;

public class BranchPhoneResponseDto : BaseDto
{
    public string? Name { get; set; }
    public string PhoneNumber { get; set; } = null!;
}
