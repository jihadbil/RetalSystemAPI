using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Branch;

public class CreateBranchDto
{
    [Required(ErrorMessage = "اسم الفرع مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم الفرع يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public List<BranchPhoneDto> Phones { get; set; } = new();
}
