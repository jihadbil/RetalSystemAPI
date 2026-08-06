using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Catalog.Unit;

public class UnitResponseDto : BaseDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int UnitPackage { get; set; }
}
