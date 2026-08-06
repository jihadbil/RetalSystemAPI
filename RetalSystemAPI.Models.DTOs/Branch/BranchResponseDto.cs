using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Branch;

public class BranchResponseDto : BaseDto
{
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public List<BranchPhoneResponseDto> Phones { get; set; } = new();
}
