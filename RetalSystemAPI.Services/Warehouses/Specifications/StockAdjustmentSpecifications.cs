using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Services.Warehouses.Specifications;

/// <summary>
/// مواصفات استعلامات التسويات الجردية مع تضمين المستودع وبنود التسوية والأصناف والباركودات.
/// </summary>
public class StockAdjustmentWithDetailsSpec : BaseSpecification<StockAdjustment>
{
    /// <summary>
    /// جلب كافة التسويات الجردية مرتبة تنازلياً بتاريخ التسوية مع تفاصيل البنود والأصناف.
    /// </summary>
    public StockAdjustmentWithDetailsSpec()
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);

        // تضمين بيانات منتجات البنود
        AddInclude("Items.Product");

        // تضمين بيانات باركودات البنود
        AddInclude("Items.ProductBarCode");

        // الترتيب تنازلياً بتاريخ التسوية
        ApplyOrderByDescending(s => s.AdjustmentDate);
    }

    /// <summary>
    /// جلب تسوية جردية محددة بواسطة المعرف مع تفاصيلها.
    /// </summary>
    /// <param name="id">المعرف الفريد للتسوية الجردية</param>
    public StockAdjustmentWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);

        // تضمين بيانات منتجات البنود
        AddInclude("Items.Product");

        // تضمين بيانات باركودات البنود
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>
    /// جلب تسوية جردية بواسطة رقم سند التسوية مع تفاصيلها.
    /// </summary>
    /// <param name="adjustmentNumber">رقم سند التسوية الجردية</param>
    public StockAdjustmentWithDetailsSpec(string adjustmentNumber) : base(s => s.AdjustmentNumber == adjustmentNumber)
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);

        // تضمين بيانات منتجات البنود
        AddInclude("Items.Product");

        // تضمين بيانات باركودات البنود
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>
    /// فلترة التسويات الجردية حسب المستودع وسبب التسوية والنطاق الزمني والبحث برقم التسوية.
    /// </summary>
    /// <param name="warehouseId">معرف المستودع (اختياري)</param>
    /// <param name="reason">سبب التسوية الجردية (اختياري)</param>
    /// <param name="fromDate">تاريخ البداية (اختياري)</param>
    /// <param name="toDate">تاريخ النهاية (اختياري)</param>
    /// <param name="search">نص البحث برقم التسوية (اختياري)</param>
    public StockAdjustmentWithDetailsSpec(
        Guid? warehouseId,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null)
        : base(s => (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value) &&
                    (!reason.HasValue || s.Reason == reason.Value) &&
                    (!fromDate.HasValue || s.AdjustmentDate >= fromDate.Value) &&
                    (!toDate.HasValue || s.AdjustmentDate <= toDate.Value) &&
                    (string.IsNullOrWhiteSpace(search) || s.AdjustmentNumber.Contains(search)))
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);

        // تضمين بيانات منتجات البنود
        AddInclude("Items.Product");

        // تضمين بيانات باركودات البنود
        AddInclude("Items.ProductBarCode");

        // الترتيب تنازلياً بتاريخ التسوية
        ApplyOrderByDescending(s => s.AdjustmentDate);
    }
}
