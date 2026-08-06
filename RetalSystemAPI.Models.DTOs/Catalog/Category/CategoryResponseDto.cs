using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Catalog.Category;

public class CategoryResponseDto : BaseDto
{
    public string Name { get; set; } = null!;
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public int ProductCount { get; set; }
    public List<CategoryResponseDto> SubCategories { get; set; } = new();
}
