using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.Services.Sales.Specifications;

/// <summary>
/// تخصيصات استعلامات مرتجعات المبيعات مع تفاصيل الفاتورة الأصلية والفرع والمستودع والعميل والبنود.
/// </summary>
public class SalesReturnWithDetailsSpec : BaseSpecification<SalesReturn>
{
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

    public SalesReturnWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude(s => s.OriginalInvoice!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    public SalesReturnWithDetailsSpec(string returnNumber) : base(s => s.ReturnNumber == returnNumber)
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude(s => s.OriginalInvoice!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

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
