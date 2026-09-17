using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;

/// <summary>
/// ناقل بيانات تحديث حالة وملاحظات أمر التحويل المخزني (Update Stock Transfer DTO).
/// </summary>
public class UpdateStockTransferDto
{
    /// <summary>المعرف الفريد لأمر التحويل المطلوب تعديله</summary>
    [Required(ErrorMessage = "معرف أمر التحويل مطلوب")]
    public Guid Id { get; set; }

    /// <summary>الحالة الجديدة لأمر التحويل (تأكيد، إلغاء)</summary>
    public StockTransferStatus Status { get; set; }

    /// <summary>ملاحظات إضافية</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}
