using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;

/// <summary>
/// ناقل بيانات إنشاء أمر تحويل مخزني بين المستودعات (Create Stock Transfer DTO).
/// </summary>
public class CreateStockTransferDto
{
    /// <summary>رقم أمر التحويل الفريد</summary>
    [Required(ErrorMessage = "رقم أمر التحويل مطلوب")]
    [MaxLength(50, ErrorMessage = "رقم أمر التحويل يجب أن لا يتجاوز 50 حرف")]
    public string TransferNumber { get; set; } = null!;

    /// <summary>تاريخ وتوقيت التحويل</summary>
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;

    /// <summary>معرف المستودع أو الفرع المصدر (المحوَّل منه)</summary>
    [Required(ErrorMessage = "المستودع المصدر مطلوب")]
    public Guid FromWarehouseId { get; set; }

    /// <summary>معرف المستودع أو الصالة الوجهة (المحوَّل إليه)</summary>
    [Required(ErrorMessage = "المستودع الوجهة مطلوب")]
    public Guid ToWarehouseId { get; set; }

    /// <summary>ملاحظات إضافية على أمر التحويل</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    /// <summary>قائمة بنود الأصناف المراد تحويلها</summary>
    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل للتحويل")]
    public List<StockTransferItemDto> Items { get; set; } = new();
}
