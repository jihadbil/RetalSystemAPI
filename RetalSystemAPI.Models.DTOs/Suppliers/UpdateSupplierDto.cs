using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

public class UpdateSupplierDto
{
    [Required(ErrorMessage = "معرف المورد مطلوب")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "اسم المورد مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم المورد يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    [MaxLength(500, ErrorMessage = "العنوان يجب أن لا يتجاوز 500 حرف")]
    public string? Address { get; set; }

    public List<SupplierPhoneDto> Phones { get; set; } = new();
}
