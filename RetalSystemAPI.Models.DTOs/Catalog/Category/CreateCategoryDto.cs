using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.Category;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "اسم التصنيف مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم التصنيف يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}
