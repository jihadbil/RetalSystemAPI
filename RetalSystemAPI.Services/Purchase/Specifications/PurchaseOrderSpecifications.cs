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
    /// <summary>جلب كافة أوامر الشراء مرتبة تنازلياً بتاريخ الطلبية</summary>
    public PurchaseOrderWithDetailsSpec()
    {
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse!);
        AddInclude(p => p.Supplier!);
        AddInclude("Items.ProductBarCode.Product");
        ApplyOrderByDescending(p => p.OrderDate);
    }

    /// <summary>جلب أمر شراء محدد بالمعرف مع تفاصيله</summary>
    /// <param name="id">معرف أمر الشراء</param>
    public PurchaseOrderWithDetailsSpec(Guid id) : base(p => p.Id == id)
    {
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse!);
        AddInclude(p => p.Supplier!);
        AddInclude("Items.ProductBarCode.Product");
    }

    /// <summary>فلترة أوامر الشراء حسب الفرع والمستودع والحالة والبحث برقم الطلبية</summary>
    public PurchaseOrderWithDetailsSpec(Guid? branchId, Guid? warehouseId, PurchaseOrderStatus? status, string? search = null)
        : base(p => (!branchId.HasValue || p.BranchId == branchId.Value) &&
                    (!warehouseId.HasValue || p.WarehouseId == warehouseId.Value) &&
                    (!status.HasValue || p.Status == status.Value) &&
                    (string.IsNullOrWhiteSpace(search) || p.OrderNumber.Contains(search)))
    {
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse!);
        AddInclude(p => p.Supplier!);
        AddInclude("Items.ProductBarCode.Product");
        ApplyOrderByDescending(p => p.OrderDate);
    }
}

/// <summary>
/// مواصفة القوائم والترقيم لأوامر الشراء — التنقلات المباشرة وبنود الجذر فقط (لعدد البنود)
/// دون التضمين العميق Items.ProductBarCode.Product الذي تحتاجه استعلامات التفاصيل فقط.
/// </summary>
public class PurchaseOrderListSpec : BaseSpecification<PurchaseOrder>
{
    /// <summary>جلب كافة أوامر الشراء للقوائم مرتبة تنازلياً بتاريخ الطلبية</summary>
    public PurchaseOrderListSpec()
    {
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse!);
        AddInclude(p => p.Supplier!);
        AddInclude(p => p.Items);
        ApplyOrderByDescending(p => p.OrderDate);
    }

    /// <summary>فلترة قوائم أوامر الشراء حسب الفرع والمستودع والحالة والبحث برقم الطلبية</summary>
    public PurchaseOrderListSpec(Guid? branchId, Guid? warehouseId, PurchaseOrderStatus? status, string? search = null)
        : base(p => (!branchId.HasValue || p.BranchId == branchId.Value) &&
                    (!warehouseId.HasValue || p.WarehouseId == warehouseId.Value) &&
                    (!status.HasValue || p.Status == status.Value) &&
                    (string.IsNullOrWhiteSpace(search) || p.OrderNumber.Contains(search)))
    {
        AddInclude(p => p.Branch);
        AddInclude(p => p.Warehouse!);
        AddInclude(p => p.Supplier!);
        AddInclude(p => p.Items);
        ApplyOrderByDescending(p => p.OrderDate);
    }
}
