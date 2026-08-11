using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Warehouses;

public class UpdateWarehouseDto
{
    [Required(ErrorMessage = "معرف المخزن مطلوب")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "اسم المخزن مطلوب")]
    [MaxLength(150, ErrorMessage = "اسم المخزن يجب أن لا يتجاوز 150 حرف")]
    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }
}
