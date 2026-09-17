using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;

/// <summary>
/// ناقل بيانات بند التحويل المخزني (Stock Transfer Item DTO).
/// </summary>
public class StockTransferItemDto
{
    /// <summary>معرف الصنف</summary>
    [Required(ErrorMessage = "معرف الصنف مطلوب")]
    public Guid ProductId { get; set; }

    /// <summary>معرف الباركود إن وجد</summary>
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>الكمية المطلوب تحويلها</summary>
    [Range(1, int.MaxValue, ErrorMessage = "الكمية المحولة يجب أن تكون 1 على الأقل")]
    public int Quantity { get; set; }

    /// <summary>ملاحظات على البند</summary>
    [MaxLength(200, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 200 حرف")]
    public string? Notes { get; set; }
}
