using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Purchase;

/// <summary>
/// ناقل بيانات ملخص أمر الشراء السريع (Purchase Order Summary DTO).
/// مخصص للجداول وقوائم المتابعة السريعة.
/// </summary>
public class PurchaseOrderSummaryDto : BaseDto
{
    /// <summary>رقم أمر الشراء</summary>
    public string OrderNumber { get; set; } = null!;

    /// <summary>معرف المورد</summary>
    public Guid? SupplierId { get; set; }

    /// <summary>اسم المورد</summary>
    public string? SupplierName { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>اسم المستودع</summary>
    public string? WarehouseName { get; set; }

    /// <summary>تاريخ الطلبية</summary>
    public DateTime OrderDate { get; set; }

    /// <summary>التاريخ المتوقع للتوريد</summary>
    public DateTime? ExpectedDate { get; set; }

    /// <summary>حالة أمر الشراء</summary>
    public PurchaseOrderStatus Status { get; set; }

    /// <summary>اسم الحالة المعرب</summary>
    public string StatusName { get; set; } = null!;

    /// <summary>إجمالي القيمة</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>عدد البنود المندرجة في الطلبية</summary>
    public int ItemCount { get; set; }
}
