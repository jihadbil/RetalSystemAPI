using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.Services.Sales.Specifications;

/// <summary>
/// مواصفات استعلامات مرتجعات المبيعات مع تضمين الفاتورة الأصلية والفرع والمستودع والعميل وبنود المرتجع.
/// </summary>
public class SalesReturnWithDetailsSpec : BaseSpecification<SalesReturn>
{
    /// <summary>جلب كافة مرتجعات المبيعات مرتبة تنازلياً بتاريخ الإرجاع</summary>
    public SalesReturnWithDetailsSpec()
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude(s => s.OriginalInvoice!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
        ApplyOrderByDescending(s => s.ReturnDate);
    }

    /// <summary>جلب سجل مرتجع مبيعات محدد بالمعرف مع تفاصيله</summary>
    /// <param name="id">معرف المرتجع</param>
    public SalesReturnWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude(s => s.OriginalInvoice!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>جلب سجل مرتجع مبيعات بواسطة رقم الإرجاع</summary>
    /// <param name="returnNumber">رقم المرتجع</param>
    public SalesReturnWithDetailsSpec(string returnNumber) : base(s => s.ReturnNumber == returnNumber)
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude(s => s.OriginalInvoice!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>فلترة مرتجعات المبيعات حسب الفرع والمستودع والعميل وسبب الإرجاع والتاريخ والبحث</summary>
    public SalesReturnWithDetailsSpec(
        Guid? branchId,
        Guid? warehouseId,
        Guid? customerId,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null)
        : base(s => (!branchId.HasValue || s.BranchId == branchId.Value) &&
                    (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value) &&
                    (!customerId.HasValue || s.CustomerId == customerId.Value) &&
                    (!reason.HasValue || s.Reason == reason.Value) &&
                    (!fromDate.HasValue || s.ReturnDate >= fromDate.Value) &&
                    (!toDate.HasValue || s.ReturnDate <= toDate.Value) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     s.ReturnNumber.Contains(search) ||
                     (s.Customer != null && s.Customer.Name.Contains(search))))
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude(s => s.OriginalInvoice!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
        ApplyOrderByDescending(s => s.ReturnDate);
    }
}

/// <summary>
/// مواصفة القوائم والترقيم لمرتجعات المبيعات — التنقلات المباشرة وبنود الجذر فقط (لعدد البنود)
/// دون التضمينات العميقة Items.Product / Items.ProductBarCode التي تحتاجها استعلامات التفاصيل فقط.
/// </summary>
public class SalesReturnListSpec : BaseSpecification<SalesReturn>
{
    /// <summary>جلب كافة مرتجعات المبيعات للقوائم مرتبة تنازلياً بتاريخ المرتجع</summary>
    public SalesReturnListSpec()
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude(s => s.OriginalInvoice!);
        AddInclude(s => s.Items);
        ApplyOrderByDescending(s => s.ReturnDate);
    }

    /// <summary>فلترة قوائم مرتجعات المبيعات حسب الفرع والمستودع والعميل وسبب الإرجاع والتاريخ والبحث</summary>
    public SalesReturnListSpec(
        Guid? branchId,
        Guid? warehouseId,
        Guid? customerId,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null)
        : base(s => (!branchId.HasValue || s.BranchId == branchId.Value) &&
                    (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value) &&
                    (!customerId.HasValue || s.CustomerId == customerId.Value) &&
                    (!reason.HasValue || s.Reason == reason.Value) &&
                    (!fromDate.HasValue || s.ReturnDate >= fromDate.Value) &&
                    (!toDate.HasValue || s.ReturnDate <= toDate.Value) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     s.ReturnNumber.Contains(search) ||
                     (s.Customer != null && s.Customer.Name.Contains(search))))
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude(s => s.OriginalInvoice!);
        AddInclude(s => s.Items);
        ApplyOrderByDescending(s => s.ReturnDate);
    }
}
