using RetalSystemAPI.Models.DTOs.Common;
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;

namespace RetalSystemAPI.Models.DTOs.Catalog.Product;

/// <summary>
/// ناقل بيانات إنشاء منتج جديد (Create Product DTO).
/// يتضمن البيانات الأساسية للمنتج، الأسعار، التصنيف، والتهيئة المبدئية للأرصدة في الصالات والمخازن والباركودات.
/// </summary>
public class CreateProductDto
{
    /// <summary>اسم المنتج الجديد</summary>
    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم المنتج يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>وصف تفصيلي للمنتج</summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>سعر تكلفة الشراء المبدئي</summary>
    [Range(0, double.MaxValue, ErrorMessage = "سعر التكلفة يجب أن يكون أكبر من أو يساوي 0")]
    [MoneyAmount]
    public decimal CostPrice { get; set; }

    /// <summary>سعر بيع التجزئة الافتراضي</summary>
    [Range(0, double.MaxValue, ErrorMessage = "سعر البيع يجب أن يكون أكبر من أو يساوي 0")]
    [MoneyAmount]
    public decimal SalePrice { get; set; }

    /// <summary>المعرف الفريد للتصنيف التابع له المنتج</summary>
    [Required(ErrorMessage = "معرف التصنيف مطلوب")]
    [NotEmptyGuid]
    public Guid CategoryId { get; set; }

    /// <summary>الكمية المبدئية لتهيئة رصيد صالة العرض</summary>
    [Range(0, int.MaxValue, ErrorMessage = "الكمية المبدئية في الصالة يجب أن تكون أكبر من أو تساوي 0")]
    public int InitialShowroomQuantity { get; set; } = 0;

    /// <summary>معرف مستودع صالة العرض المستهدف</summary>
    public Guid? ShowroomWarehouseId { get; set; }

    /// <summary>معرف المخزن الرئيسي المستهدف</summary>
    public Guid? StorageWarehouseId { get; set; }

    /// <summary>قائمة كميات صالات العرض المبدئية الموزعة</summary>
    [NoNullItems]
    public List<CreateShowroomStockQuantityDto> ShowroomInitialQuantities { get; set; } = new();

    /// <summary>قائمة كميات المخازن المبدئية الموزعة حسب الباركود</summary>
    [NoNullItems]
    public List<CreateStorageStockQuantityDto> StorageInitialQuantities { get; set; } = new();

    /// <summary>قائمة الباركودات المراد إنشاؤها وربطها بالمنتج</summary>
    [NoNullItems]
    public List<CreateProductBarCodeDto> BarCodes { get; set; } = new();

    /// <summary>قائمة وحدات القياس المربوطة بالمنتج</summary>
    [NoNullItems]
    public List<CreateProductUnitDto> Units { get; set; } = new();
}

/// <summary>
/// ناقل بيانات تحديد الكمية المبدئية في صالة عرض محددة.
/// </summary>
public class CreateShowroomStockQuantityDto
{
    /// <summary>معرف مستودع صالة العرض</summary>
    [NotEmptyGuid]
    public Guid WarehouseId { get; set; }

    /// <summary>الكمية المبدئية</summary>
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
}

/// <summary>
/// ناقل بيانات تحديد الكمية المبدئية في مخزن رئيسي محدد لباركود معين.
/// </summary>
public class CreateStorageStockQuantityDto
{
    /// <summary>معرف المخزن الرئيسي</summary>
    [NotEmptyGuid]
    public Guid WarehouseId { get; set; }

    /// <summary>رمز الباركود</summary>
    [Required]
    [MaxLength(100)]
    public string BarCode { get; set; } = null!;

    /// <summary>الكمية المبدئية</summary>
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
}
