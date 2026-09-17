using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StorgeStock;

/// <summary>
/// ناقل بيانات استجابة رصيد المخزن الرئيسي (Storage Stock Response DTO).
/// يتضمن اسم المخزن والصنف ومسمى الباركود وقيمته والصورة والكمية ورصد النقص.
/// </summary>
public class StorgeStockResponseDto : BaseDto
{
    /// <summary>معرف المخزن الرئيسي</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>اسم المخزن</summary>
    public string WarehouseName { get; set; } = null!;

    /// <summary>معرف باركود الصنف</summary>
    public Guid ProductBarcodeId { get; set; }

    /// <summary>عنوان ومسمى الباركود (العبوة أو الصنف)</summary>
    public string BarcodeTitle { get; set; } = null!;

    /// <summary>رمز الباركود</summary>
    public string BarcodeValue { get; set; } = null!;

    /// <summary>اسم المنتج</summary>
    public string ProductName { get; set; } = null!;

    /// <summary>رابط صورة الباركود أو الصنف</summary>
    public string? ImageUrl { get; set; }

    /// <summary>الكمية المتوفرة بالمخزن</summary>
    public decimal Quantity { get; set; }

    /// <summary>الحد الأدنى المطلوب بالمخزن (حد الطلب)</summary>
    public int MinStockLevel { get; set; }

    /// <summary>هل الرصيد أقل من الحد الأدنى</summary>
    public bool IsBelowMinLevel { get; set; }
}
