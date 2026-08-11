using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses;

public class CreateWarehouseDto
{
    [Required(ErrorMessage = "اسم المخزن مطلوب")]
    [MaxLength(150, ErrorMessage = "اسم المخزن يجب أن لا يتجاوز 150 حرف")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }

    [Required(ErrorMessage = "نوع المخزن مطلوب")]
    public WarehouseType Type { get; set; }

    public bool IsActive { get; set; } = true;
}
