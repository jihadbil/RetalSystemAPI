using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.Services.Purchase.Specifications;

/// <summary>
/// مواصفة استعلام أوامر الشراء مع تضمين الفرع، المستودع، وبنود الطلبية مع بيانات الأصناف والباركود.
/// </summary>
public class PurchaseOrderWithDetailsSpec : BaseSpecification<PurchaseOrder>
{
    /// <summary>
    /// جلب كافة أوامر الشراء مرتبة تنازلياً بتاريخ الطلبية مع تضمين العلاقات التفصيلية.
    /// </summary>
    public PurchaseOrderWithDetailsSpec()
    {
        // تضمين بيانات الفرع الذي صدر منه أمر الشراء
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع أو المخزن المستلم
        AddInclude(p => p.Warehouse!);
        // تضمين بيانات المورد المطلوب منه البضاعة
        AddInclude(p => p.Supplier!);
        // تضمين تفاصيل الأصناف والباركود المرتبطة بكل بند في أمر الشراء
        AddInclude("Items.ProductBarCode.Product");
        // ترتيب أوامر الشراء تنازلياً حسب تاريخ الطلبية
        ApplyOrderByDescending(p => p.OrderDate);
    }

    /// <summary>
    /// جلب أمر شراء محدد بالمعرف مع تفاصيله الكاملة.
    /// </summary>
    /// <param name="id">معرف أمر الشراء الفريد</param>
    public PurchaseOrderWithDetailsSpec(Guid id) : base(p => p.Id == id)
    {
        // تضمين بيانات الفرع التابع له أمر الشراء
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع المستهدف بالاستلام
        AddInclude(p => p.Warehouse!);
        // تضمين بيانات المورد
        AddInclude(p => p.Supplier!);
        // تضمين تفاصيل الأصناف والباركود لكل بند
        AddInclude("Items.ProductBarCode.Product");
    }

    /// <summary>
    /// فلترة أوامر الشراء حسب الفرع والمستودع والحالة والبحث برقم الطلبية.
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع للفلترة (اختياري)</param>
    /// <param name="status">حالة أمر الشراء (اختياري)</param>
    /// <param name="search">نص البحث في رقم الطلبية (اختياري)</param>
    public PurchaseOrderWithDetailsSpec(Guid? branchId, Guid? warehouseId, PurchaseOrderStatus? status, string? search = null)
        : base(p => (!branchId.HasValue || p.BranchId == branchId.Value) &&
                    (!warehouseId.HasValue || p.WarehouseId == warehouseId.Value) &&
                    (!status.HasValue || p.Status == status.Value) &&
                    (string.IsNullOrWhiteSpace(search) || p.OrderNumber.Contains(search)))
    {
        // تضمين بيانات الفرع
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع
        AddInclude(p => p.Warehouse!);
        // تضمين بيانات المورد
        AddInclude(p => p.Supplier!);
        // تضمين تفاصيل الأصناف والباركود لكل بند
        AddInclude("Items.ProductBarCode.Product");
        // ترتيب النتائج تنازلياً حسب تاريخ الطلبية
        ApplyOrderByDescending(p => p.OrderDate);
    }
}

/// <summary>
/// مواصفة القوائم والترقيم لأوامر الشراء — التنقلات المباشرة وبنود الجذر فقط (لعدد البنود)
/// دون التضمين العميق Items.ProductBarCode.Product الذي تحتاجه استعلامات التفاصيل فقط.
/// </summary>
public class PurchaseOrderListSpec : BaseSpecification<PurchaseOrder>
{
    /// <summary>
    /// جلب كافة أوامر الشراء للقوائم مرتبة تنازلياً بتاريخ الطلبية.
    /// </summary>
    public PurchaseOrderListSpec()
    {
        // تضمين بيانات الفرع
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع
        AddInclude(p => p.Warehouse!);
        // تضمين بيانات المورد
        AddInclude(p => p.Supplier!);
        // تضمين قائمة البنود السطحية لحساب العدد والإجماليات الخفيفة
        AddInclude(p => p.Items);
        // ترتيب النتائج تنازلياً حسب تاريخ الطلبية
        ApplyOrderByDescending(p => p.OrderDate);
    }

    /// <summary>
    /// فلترة قوائم أوامر الشراء حسب الفرع والمستودع والحالة والبحث برقم الطلبية.
    /// </summary>
    /// <param name="branchId">معرف الفرع (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع (اختياري)</param>
    /// <param name="status">حالة أمر الشراء (اختياري)</param>
    /// <param name="search">نص البحث في رقم الطلبية (اختياري)</param>
    public PurchaseOrderListSpec(Guid? branchId, Guid? warehouseId, PurchaseOrderStatus? status, string? search = null)
        : base(p => (!branchId.HasValue || p.BranchId == branchId.Value) &&
                    (!warehouseId.HasValue || p.WarehouseId == warehouseId.Value) &&
                    (!status.HasValue || p.Status == status.Value) &&
                    (string.IsNullOrWhiteSpace(search) || p.OrderNumber.Contains(search)))
    {
        // تضمين بيانات الفرع
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع
        AddInclude(p => p.Warehouse!);
        // تضمين بيانات المورد
        AddInclude(p => p.Supplier!);
        // تضمين البنود لحساب الأعداد
        AddInclude(p => p.Items);
        // ترتيب النتائج تنازلياً حسب تاريخ الطلبية
        ApplyOrderByDescending(p => p.OrderDate);
    }
}
