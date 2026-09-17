using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Purchase;

/// <summary>
/// ناقل بيانات استجابة تفاصيل أمر الشراء (Purchase Order Response DTO).
/// يتضمن بيانات المورد والفرع والمستودع والحالة والإجمالي وقائمة البنود.
/// </summary>
public class PurchaseOrderResponseDto : BaseDto
{
    /// <summary>رقم أمر الشراء</summary>
    public string OrderNumber { get; set; } = null!;

    /// <summary>معرف المورد</summary>
    public Guid? SupplierId { get; set; }

    /// <summary>اسم المورد</summary>
    public string? SupplierName { get; set; }

    /// <summary>معرف الفرع الطالب</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>معرف المستودع المستلم</summary>
    public Guid? WarehouseId { get; set; }

    /// <summary>اسم المستودع</summary>
    public string? WarehouseName { get; set; }

    /// <summary>تاريخ الطلبية</summary>
    public DateTime OrderDate { get; set; }

    /// <summary>تاريخ التوريد المتوقع</summary>
    public DateTime? ExpectedDate { get; set; }

    /// <summary>حالة أمر الشراء التعدادية</summary>
    public PurchaseOrderStatus Status { get; set; }

    /// <summary>اسم حالة أمر الشراء</summary>
    public string StatusName { get; set; } = null!;

    /// <summary>إجمالي القيمة المالية المقدرة للطلبية</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>قائمة بنود الأصناف في أمر الشراء</summary>
    public List<PurchaseOrderItemResponseDto> Items { get; set; } = new();
}
